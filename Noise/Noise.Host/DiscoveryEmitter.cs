using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.Peer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host
{
    internal class DiscoveryEmitter
    {
        private readonly PeerConfiguration _peerConfiguration;
        private readonly IOutputMonitor _outputMonitor;

        public DiscoveryEmitter(PeerConfiguration peerConfiguration, IOutputMonitor outputMonitor)
        {
            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                LogVerbose("Discovery emitter started.");

                if (_peerConfiguration.Preferences.BroadcastDiscoveryOnStartup)
                    await PerformDiscovery(cancellationToken);

                if (!_peerConfiguration.Preferences.BroadcastDiscoveryPeriodically)
                    return;

                var timer = new PeriodicTimer(TimeSpan.FromMinutes(_peerConfiguration.Preferences.PeriodicallyDiscoveryIntervalMinutes));

                while (await timer.WaitForNextTickAsync(cancellationToken))
                    await PerformDiscovery(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                LogVerbose("Discovery emitter thread task canceled.");
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
            finally
            {
                LogVerbose("Discovery emitter stopped.");
            }        
        }

        private async Task PerformDiscovery(CancellationToken cancellationToken)
        {
            LogVerbose("Discovery broadcast performance started.");

            foreach (var endpoint in _peerConfiguration.GetEndpoints(true))
            {
                LogVerbose($"Emitting discovery packets for: {endpoint.Endpoint}.");

                foreach (var peer in _peerConfiguration.GetPeers())
                {
                    using var client = new NoiseClient(endpoint.Endpoint, _outputMonitor, _peerConfiguration, new NoiseClientConfiguration
                    {
                        VerboseMode = _peerConfiguration.Preferences.VerboseMode,
                        StreamBufferSize = _peerConfiguration.Preferences.ClientStreamBufferSize,
                        ConnectTimeoutMs = _peerConfiguration.Preferences.ClientConnectTimeoutMs,
                        ReadTimeoutMs = _peerConfiguration.Preferences.ClientReadTimeoutMs,
                        MaxConnectRetryCount = _peerConfiguration.Preferences.ClientMaxConnectRetryCount
                    });

                    LogVerbose($"Discovery emisssion target: {peer.PublicKey}");

                    await client.SendDiscovery(peer.PublicKey, cancellationToken);
                }
            }

            LogVerbose("Discovery broadcast performance finished.");
        }

        private void LogVerbose(string message)
        {
            if (!_peerConfiguration.Preferences.VerboseMode) return;
            
            _outputMonitor.LogInformation(message);
        }
    }
}
