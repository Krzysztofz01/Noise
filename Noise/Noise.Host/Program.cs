using Noise.Core.Abstraction;
using Noise.Core.Exceptions;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Server;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host
{
    class Program
    {
        private const int SUCCESS = 0;
        private const int FAILURE = 1;

        public static IOutputMonitor OutputMonitor;
        public static ICommandHandler CommandHandler;
        public static PeerConfiguration PeerConfiguration;

        private static async Task<int> Main(string[] args)
        {
            try
            {
                // Initialization
                await InitializeServices();
                var cts = new CancellationTokenSource();

                // Server
                using INoiseServer server = new NoiseServer(OutputMonitor, PeerConfiguration);
                _ = Task.Run(async () => await server.StartAsync(cts.Token));

                // Program loop
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        CommandHandler.Prefix();
                        await CommandHandler.Execute(Console.ReadLine(), cts);
                    }
                    catch (CommandHandlerException ex)
                    {
                        OutputMonitor.LogError(ex);
                    }
                    catch (Exception ex)
                    {
                        OutputMonitor.LogError("Unexpected failure.");
                        cts.Cancel();
                    }
                }

                server.Stop();
                Thread.Sleep(1500);

                await FileHandler.SavePeerConfigurationCipher(PeerConfiguration);
                Thread.Sleep(1500);

                return SUCCESS;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal application failure.");
                Console.WriteLine(ex.Message);
                return FAILURE;
            }
        }

        private async static Task InitializeServices()
        {
            OutputMonitor = new OutputMonitor();
            CommandHandler = new CommandHandler(OutputMonitor);

            string peerPassword = ConsoleUtility.ReadSecret("Peer password: ");
            PeerConfiguration = await GetPeerConfiguration(peerPassword);
        }

        private async static Task<PeerConfiguration> GetPeerConfiguration(string peerPassword)
        {
            try
            {
                if (FileHandler.PeerConfigurationFileExists())
                {
                    var peerConfigurationCipher = FileHandler.GetPeerConfigurationCipher();

                    return PeerEncryption.DecryptPeerConfiguration(peerConfigurationCipher, peerPassword) ??
                        throw new PeerDataException(PeerDataProblemType.WRONG_PEER_SECRET);
                }

                OutputMonitor.LogInformation("No peer file found. New peer created with the provided password.");
                var initializedPeerConfiguration = PeerConfiguration.Factory.Initialize(peerPassword);

                await FileHandler.SavePeerConfigurationCipher(initializedPeerConfiguration);

                return initializedPeerConfiguration;
            }
            catch (Exception ex)
            {
                OutputMonitor.LogError(ex);
                throw;
            }
        }
    }
}
