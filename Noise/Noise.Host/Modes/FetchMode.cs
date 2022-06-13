using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Host.Abstraction;
using System;
using System.Threading.Tasks;

namespace Noise.Host.Modes
{
    internal class FetchMode : IProgramMode
    {
        public const string Command = "--fetch";

        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;

        public FetchMode(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration)
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
                _outputMonitor.Clear();
                ((OutputMonitor)_outputMonitor).WriteRaw(Title.AsciiTitle, ConsoleColor.DarkGreen, false);
                ((OutputMonitor)_outputMonitor).WriteRaw($"{Environment.NewLine}{Constants.Version}{Environment.NewLine}", ConsoleColor.Green);

                ((OutputMonitor)_outputMonitor).WriteRaw("Local peer public key:", ConsoleColor.Green);
                ((OutputMonitor)_outputMonitor).WriteRaw(_peerConfiguration.Secrets.PublicKey, ConsoleColor.Yellow);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError($"{Environment.NewLine}Unexpected failure.", ex);
            }

            return await Task.FromResult(false);
        }
    }
}
