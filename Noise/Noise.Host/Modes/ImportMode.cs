using Noise.Core.Abstraction;
using Noise.Core.File;
using Noise.Core.Peer;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noise.Host.Modes
{
    internal class ImportMode : IProgramMode
    {
        public const string Command = "--import";

        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;

        public ImportMode(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration)
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
                _outputMonitor.LogInformation("The Noise peer host started in card import mode.");

                string cardFilePath = args.Skip(1).First();

                string retrivedPublicKey = FileHandler.GetPeerCardPublicKey(cardFilePath);

                _peerConfiguration.InsertPeer(retrivedPublicKey);

                await FileHandler.SavePeerConfigurationCipher(_peerConfiguration);

                _outputMonitor.LogInformation("Noise peer successfull imported from card.");

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
