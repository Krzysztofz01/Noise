using Noise.Core.Abstraction;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Threading.Tasks;

namespace Noise.Host.Modes
{
    internal class ConfigMode : IProgramMode
    {
        public const string Command = "--config";

        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly ICommandHandler _commandHandler;

        public ConfigMode(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration, ICommandHandler commandHandler)
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
                _outputMonitor.LogInformation("The Noise peer host started in configuration mode.");

                _commandHandler.Config(args);

                await FileHandler.SavePeerConfigurationCipher(_peerConfiguration);

                return true;
            }
            catch (CommandHandlerException ex)
            {
                _outputMonitor.LogError($"{Environment.NewLine}{ex.Message}");
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
            }

            return false;
        }
    }
}
