using Noise.Core.Abstraction;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Threading.Tasks;

namespace Noise.Host.Modes
{
    internal class ExportMode : IProgramMode
    {
        public const string Command = "--export";

        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;

        public ExportMode(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration)
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
                _outputMonitor.LogInformation("The Noise peer host started in card export mode.");

                if (!await FileHandler.SavePeerCard(_peerConfiguration))
                    throw new CommandHandlerException("Failed to export the peer card.");

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
