using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Server;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using Noise.Host.Modes;
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

                if (args.Length != 0)
                {
                    if (args.FirstIs(ConfigMode.Command)) return await new ConfigMode(OutputMonitor, PeerConfiguration, CommandHandler)
                            .Launch(args) ? SUCCESS : FAILURE;

                    if (args.FirstIs(ImportMode.Command)) return await new ImportMode(OutputMonitor, PeerConfiguration)
                            .Launch(args) ? SUCCESS : FAILURE;

                    if (args.FirstIs(ExportMode.Command)) return await new ExportMode(OutputMonitor, PeerConfiguration)
                            .Launch(args) ? SUCCESS : FAILURE;
                }

                using INoiseServer server = new NoiseServer(OutputMonitor, PeerConfiguration, GetNoiseServerConfiguration());
                _ = Task.Run(async () => await server.StartAsync(cts.Token));

                OutputMonitor.LogInformation("The Noise peer host started.");
                Thread.Sleep(_timeOffsetMs);

                var discoveryEmitter = new DiscoveryEmitter(PeerConfiguration, OutputMonitor);
                _ = Task.Run(async () => await discoveryEmitter.Start(cts.Token));

                ((OutputMonitor)OutputMonitor).WriteRaw("Spread the noise...", ConsoleColor.Yellow);
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

        private static NoiseServerConfiguration GetNoiseServerConfiguration()
        {
            return new NoiseServerConfiguration
            {
                VerboseMode = PeerConfiguration.Preferences.VerboseMode,
                StreamBufferSize = PeerConfiguration.Preferences.ServerStreamBufferSize,
                EnableKeepAlive = PeerConfiguration.Preferences.ServerEnableKeepAlive,
                KeepAliveInterval = PeerConfiguration.Preferences.ServerKeepAliveInterval,
                KeepAliveTime = PeerConfiguration.Preferences.ServerKeepAliveTime,
                KeepAliveRetryCount = PeerConfiguration.Preferences.ServerKeepAliveRetryCount,
                EnableNatTraversal = PeerConfiguration.Preferences.EnableWindowsSpecificNatTraversal
            };
        }
    }
}
