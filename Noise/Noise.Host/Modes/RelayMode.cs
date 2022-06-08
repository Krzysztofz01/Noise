using Noise.Core.Abstraction;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Server;
using Noise.Host.Abstraction;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host.Modes
{
    internal class RelayMode : IProgramMode
    {
        public const string Command = "--relay";

        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;

        private const int _timeOffsetMs = 1500;

        public RelayMode(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));
        }

        public async Task<bool> Launch(string[] args)
        {
            try
            {
                _outputMonitor.LogInformation("The Noise peer host started in relay mode.");

                var cts = new CancellationTokenSource();

                using INoiseServer server = new NoiseServer(_outputMonitor, _peerConfiguration, GetNoiseServerConfiguration());
                _ = Task.Run(async () => await server.StartAsync(cts.Token));

                _outputMonitor.LogInformation("The Noise peer host started. Click [Esc] to quit the host.");
                Thread.Sleep(_timeOffsetMs);

                var discoveryEmitter = new DiscoveryEmitter(_peerConfiguration, _outputMonitor);
                _ = Task.Run(async () => await discoveryEmitter.Start(cts.Token));

                ((OutputMonitor)_outputMonitor).WriteRaw("Spread the noise...", ConsoleColor.Yellow);
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var input = Console.ReadKey();
                        if (input.Key == ConsoleKey.Escape) cts.Cancel();
                    }
                    catch (Exception ex)
                    {
                        _outputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
                        cts.Cancel();
                    }
                }

                _outputMonitor.LogInformation("Stopping the Noise peer host.");

                server.Stop();
                Thread.Sleep(_timeOffsetMs);

                await FileHandler.SavePeerConfigurationCipher(_peerConfiguration);
                Thread.Sleep(_timeOffsetMs);

                return true;
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
            }

            return false;
        }

        private NoiseServerConfiguration GetNoiseServerConfiguration()
        {
            return new NoiseServerConfiguration
            {
                VerboseMode = _peerConfiguration.Preferences.VerboseMode,
                StreamBufferSize = _peerConfiguration.Preferences.ServerStreamBufferSize,
                EnableKeepAlive = _peerConfiguration.Preferences.ServerEnableKeepAlive,
                KeepAliveInterval = _peerConfiguration.Preferences.ServerKeepAliveInterval,
                KeepAliveTime = _peerConfiguration.Preferences.ServerKeepAliveTime,
                KeepAliveRetryCount = _peerConfiguration.Preferences.ServerKeepAliveRetryCount,
                EnableNatTraversal = _peerConfiguration.Preferences.EnableWindowsSpecificNatTraversal,
                RelayMode = true
            };
        }
    }
}
