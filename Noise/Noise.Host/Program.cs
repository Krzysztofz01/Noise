using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using Noise.Host.Modes;
using System;
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
                await InitializeServices();
                
                if (args.Length != 0)
                {
                    if (args.FirstIs(ConfigMode.Command)) return await new ConfigMode(OutputMonitor, PeerConfiguration, CommandHandler)
                            .Launch(args) ? SUCCESS : FAILURE;

                    if (args.FirstIs(ImportMode.Command)) return await new ImportMode(OutputMonitor, PeerConfiguration)
                            .Launch(args) ? SUCCESS : FAILURE;

                    if (args.FirstIs(ExportMode.Command)) return await new ExportMode(OutputMonitor, PeerConfiguration)
                            .Launch(args) ? SUCCESS : FAILURE;

                    if (args.FirstIs(RelayMode.Command)) return await new RelayMode(OutputMonitor, PeerConfiguration)
                            .Launch(args) ? SUCCESS : FAILURE;

                    if (args.FirstIs(FetchMode.Command)) return await new FetchMode(OutputMonitor, PeerConfiguration)
                            .Launch(args) ? SUCCESS : FAILURE;
                }

                return await new DefaultMode(OutputMonitor, PeerConfiguration, CommandHandler)
                    .Launch(args) ? SUCCESS : FAILURE;
            }
            catch (Exception ex)
            {
                OutputMonitor.LogError("Fatal application failure.");
                OutputMonitor.LogError(ex.Message);
                return FAILURE;
            }
        }

        private async static Task InitializeServices()
        {
            OutputMonitor = new OutputMonitor();

            var peerConfigurationImporter = new PeerConfigurationImporter(OutputMonitor);
            
            var peerConfigurationSecret = ConsoleUtility.ReadSecret("Peer password: ");
            PeerConfiguration = await peerConfigurationImporter.ImportOrInitializePeerConfiguration(peerConfigurationSecret);

            CommandHandler = new CommandHandler(OutputMonitor, PeerConfiguration);
        }    
    }
}
