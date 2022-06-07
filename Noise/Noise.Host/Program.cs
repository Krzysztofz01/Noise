using Noise.Core.Abstraction;
using Noise.Core.Exceptions;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Protocol;
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

                    var localPeerConfiguration = PeerEncryption.DecryptPeerConfiguration(peerConfigurationCipher, peerPassword) ??
                        throw new PeerDataException(PeerDataProblemType.WRONG_PEER_SECRET);

                    if (!localPeerConfiguration.IsVersionValid(Constants.Version))
                        throw new InvalidOperationException($"Version mismatch detected. Peer: {localPeerConfiguration.Version ?? "Version undefined" }. Host: {Constants.Version}. You can enable the unsafe AllowHostVersionMismatch flag to proceed.");

                    if (Constants.Version != localPeerConfiguration.Version || localPeerConfiguration.Preferences.ForceUpdate)
                    {
                        if (localPeerConfiguration.Preferences.ForceUpdate)
                            OutputMonitor.LogInformation("Perfmorming a forced update of the peer configuration.");

                        OutputMonitor.LogInformation($"Peer version will be updated from { localPeerConfiguration.Version ?? "Version undefined"} to { Constants.Version }");
                        localPeerConfiguration.UpdatePeerVersion(Constants.Version);
                    }

                    await FileHandler.SavePeerConfigurationCipher(localPeerConfiguration);

                    return localPeerConfiguration;
                }

                OutputMonitor.LogInformation("No peer file found. New peer created with the provided password.");

                var initializedPeerConfiguration = PeerConfiguration.Factory.Initialize(peerPassword, Constants.Version);

                await FileHandler.SavePeerConfigurationCipher(initializedPeerConfiguration);

                return initializedPeerConfiguration;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
