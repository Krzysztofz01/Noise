using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Server;
using Noise.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host
{
    class Program
    {
        private const int SUCCESS = 0;
        private const int FAILURE = 1;

        private static PeerConfiguration _peerConfiguration;
        private static IOutput _output;
        private static IPacketService _packetService;

        private static async Task<int> Main(string[] args)
        {
            try
            {
                // Singleton dependencies configuration
                _peerConfiguration = await SetupPeer();
                _output = new ConsoleOutput();
                _packetService = new PacketService();

                var cts = new CancellationTokenSource();

                // Server setup
                using var server = new NoiseServer(_output, _packetService, _peerConfiguration);         
                _ = Task.Run(async () => await server.StartAsync(cts.Token));

                // Client setup
                using var client = new NoiseClient(_peerConfiguration);
                var commandHandler = new CommandHandler(_peerConfiguration, _output, _packetService, cts);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        Console.Write("> ");

                        await commandHandler.Execute(Console.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        _output.WriteException("Command exception", ex);
                    }
                }

                await FileHandler.SavePeerConfigurationFile(_peerConfiguration);
                Thread.Sleep(1000);

                return SUCCESS;
            }
            catch (Exception)
            {
                return FAILURE;
            }
        }

        private async static Task<PeerConfiguration> SetupPeer()
        {
            try
            {
                if (FileHandler.PeerConfigurationFileExists())
                {
                    return FileHandler.GetPeerConfiguration();
                }

                var peerConfiguration = PeerConfiguration.Factory.Initialize();

                await FileHandler.SavePeerConfigurationFile(peerConfiguration);

                return peerConfiguration;
            }
            catch (Exception)
            {
                Console.WriteLine("Fatal error. Can not utilize configuration.");
                throw;
            }
        }
    }
}
