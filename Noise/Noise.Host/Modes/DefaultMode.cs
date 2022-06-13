using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Core.Server;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host.Modes
{
    internal class DefaultMode : IProgramMode
    {
        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly ICommandHandler _commandHandler;

        private const int _timeOffsetMs = 1500;

        public DefaultMode(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration, ICommandHandler commandHandler)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _commandHandler = commandHandler ??
                throw new ArgumentNullException(nameof(commandHandler));
        }

        public async Task<bool> Launch(string[] args)
        {
            try
            {
                var cts = new CancellationTokenSource();

                using INoiseServer server = new NoiseServer(_outputMonitor, _peerConfiguration, GetNoiseServerConfiguration());
                _ = Task.Run(async () => await server.StartAsync(cts.Token));

                _outputMonitor.LogInformation("The Noise peer host started.");
                Thread.Sleep(_timeOffsetMs);

                var discoveryEmitter = new DiscoveryEmitter(_peerConfiguration, _outputMonitor);
                _ = Task.Run(async () => await discoveryEmitter.Start(cts.Token));

                ((OutputMonitor)_outputMonitor).WriteRaw("Spread the noise...", ConsoleColor.Yellow);
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        _commandHandler.Prefix();

                        string input = Console.ReadLine();
                        if (!input.IsEmpty()) await _commandHandler.Execute(input, cts);
                    }
                    catch (CommandHandlerException ex)
                    {
                        _outputMonitor.LogError($"{Environment.NewLine}{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _outputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
                        cts.Cancel();
                    }
                }

                server.Stop();
                Thread.Sleep(_timeOffsetMs);

                await FileHandler.SavePeerConfigurationCipher(_peerConfiguration);
                Thread.Sleep(_timeOffsetMs);

                return true;
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError("Fatal application failure.");
                _outputMonitor.LogError(ex.Message);
                return false;
            }
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
                RelayMode = false
            };
        }
    }
}
