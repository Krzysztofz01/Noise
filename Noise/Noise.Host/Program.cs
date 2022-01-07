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

                var ct = new CancellationToken(false);

                // Server setup
                using var server = new NoiseServer(_output, _packetService, _peerConfiguration);         
                _ = Task.Run(async () => await server.StartAsync(ct));

                // Client setup
                using var client = new NoiseClient(_peerConfiguration);

                while (!ct.IsCancellationRequested)
                {
                    Console.Write("> ");
                    string inputCommand = Console.ReadLine();
                    await ParseInput(inputCommand, client, _output, _peerConfiguration, ct);
                }

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

        private async static Task ParseInput(string input, INoiseClient noiseClient, IOutput output, PeerConfiguration peerConfiguration, CancellationToken ct)
        {
            output.WriteLog(input);
            throw new NotImplementedException();
        }
    }
}
