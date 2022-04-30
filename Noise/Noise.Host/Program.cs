using Noise.Core.Abstraction;
using Noise.Core.Exceptions;
using Noise.Core.Extensions;
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
        private const int _timeOffsetMs = 1500;

        public static IOutputMonitor OutputMonitor;
        public static ICommandHandler CommandHandler;
        public static PeerConfiguration PeerConfiguration;

        private static async Task<int> Main(string[] args)
        {
            try
            {
                await InitializeServices();
                var cts = new CancellationTokenSource();

                if (args.FirstIs("--config"))
                {
                    try
                    {
                        OutputMonitor.LogInformation("The Noise peer host started in configuration mode.");

                        CommandHandler.Config(args);

                        await FileHandler.SavePeerConfigurationCipher(PeerConfiguration);
                  
                        return SUCCESS;

                    }
                    catch (CommandHandlerException ex)
                    {
                        OutputMonitor.LogError($"{Environment.NewLine}{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        OutputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
                        cts.Cancel();

                        return FAILURE;
                    }
                }

                using INoiseServer server = new NoiseServer(OutputMonitor, PeerConfiguration, GetNoiseServerConfiguration());
                _ = Task.Run(async () => await server.StartAsync(cts.Token));

                ((OutputMonitor)OutputMonitor).WriteRaw("Spread the noise...", ConsoleColor.Yellow);
                OutputMonitor.LogInformation("The Noise peer host started.");
                Thread.Sleep(_timeOffsetMs);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        CommandHandler.Prefix();

                        string input = Console.ReadLine();
                        if(!input.IsEmpty()) await CommandHandler.Execute(input, cts);
                    }
                    catch (CommandHandlerException ex)
                    {
                        OutputMonitor.LogError($"{Environment.NewLine}{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        OutputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
                        cts.Cancel();
                    }
                }

                server.Stop();
                Thread.Sleep(_timeOffsetMs);

                await FileHandler.SavePeerConfigurationCipher(PeerConfiguration);
                Thread.Sleep(_timeOffsetMs);

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
            
            string peerPassword = ConsoleUtility.ReadSecret("Peer password: ");
            PeerConfiguration = await GetPeerConfiguration(peerPassword);

            CommandHandler = new CommandHandler(OutputMonitor, PeerConfiguration);
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

        private static NoiseServerConfiguration GetNoiseServerConfiguration()
        {
            return new NoiseServerConfiguration
            {
                VerboseMode = PeerConfiguration.Preferences.VerboseMode
            };
        }
    }
}
