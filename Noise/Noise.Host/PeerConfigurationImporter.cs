using Noise.Core.Abstraction;
using Noise.Core.Exceptions;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.Threading.Tasks;

namespace Noise.Host
{
    internal class PeerConfigurationImporter
    {
        private readonly IOutputMonitor _outputMonitor;

        public PeerConfigurationImporter(IOutputMonitor outputMonitor)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));
        }

        public async Task<PeerConfiguration> ImportOrInitializePeerConfiguration(string peerConfigurationSecret)
        {
            try
            {
                if (FileHandler.PeerConfigurationFileExists())
                {
                    var peerConfigurationCipher = FileHandler.GetPeerConfigurationCipher();

                    var localPeerConfiguration = PeerEncryption.DecryptPeerConfiguration(peerConfigurationCipher, peerConfigurationSecret) ??
                        throw new PeerDataException(PeerDataProblemType.WRONG_PEER_SECRET);

                    if (!localPeerConfiguration.IsVersionValid(Constants.Version))
                        throw new InvalidOperationException($"Version mismatch detected. Peer: {localPeerConfiguration.Version ?? "Version undefined" }. Host: {Constants.Version}. You can enable the unsafe AllowHostVersionMismatch flag to proceed.");

                    if (Constants.Version != localPeerConfiguration.Version || localPeerConfiguration.Preferences.ForceUpdate)
                    {
                        if (localPeerConfiguration.Preferences.ForceUpdate)
                            _outputMonitor.LogInformation("Perfmorming a forced update of the peer configuration.");

                        _outputMonitor.LogInformation($"Peer version will be updated from { localPeerConfiguration.Version ?? "Version undefined"} to { Constants.Version }");
                        localPeerConfiguration.UpdatePeerVersion(Constants.Version);
                    }

                    await FileHandler.SavePeerConfigurationCipher(localPeerConfiguration);

                    return localPeerConfiguration;
                }

                _outputMonitor.LogInformation("No peer file found. New peer created with the provided password.");

                var initializedPeerConfiguration = PeerConfiguration.Factory.Initialize(peerConfigurationSecret, Constants.Version);

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
