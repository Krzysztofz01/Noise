using Noise.Core.Abstraction;
using Noise.Core.Peer;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host
{
    public class CommandHandler
    {
        private readonly PeerConfiguration _peerConfiguration;
        private readonly IOutput _output;
        private readonly IPacketService _packetService;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CommandHandler(PeerConfiguration peerConfiguration, IOutput output, IPacketService packetService, CancellationTokenSource cancellationTokenSource)
        {
            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _output = output ??
                throw new ArgumentNullException(nameof(output));

            _packetService = packetService ??
                throw new ArgumentNullException(nameof(packetService));

            _cancellationTokenSource = cancellationTokenSource ??
                throw new ArgumentNullException(nameof(cancellationTokenSource));
        }

        public async Task Execute(string command)
        {
            var commandArr = command.Trim().Split(" ");
            if (!commandArr.Any()) return;

            var type = commandArr.First().ToUpper();
            var args = commandArr.Skip(1).ToArray();

            switch (type)
            {
                case "EXIT": ExecuteExit(); return;
                case "CLEAR": ExecuteClear(); return;
                case "LIST": ExecuteList(args); return;

                default: throw new InvalidOperationException("Invalid command.");
            }
        }

        private void ExecuteClear()
        {
            ((ConsoleOutput)_output).Clear();
        }

        private void ExecuteExit()
        {
            _output.WriteRaw("Closing connections...");
            _cancellationTokenSource.Cancel();
        }

        private void ExecuteList(string[] args)
        {
            string usage = "Command usage: list <keys, aliases, endpoints>";

            if (!args.Any())
            {
                _output.WriteRaw(usage);
                return;
            }

            string arg = args.First().ToUpper();

            if (arg == "KEYS" || arg == "ALIASES")
            {
                var keys = _peerConfiguration.GetKeys();
                if (!keys.Any())
                {
                    _output.WriteRaw("No available keys and aliases");
                    return;
                }

                _output.WriteRaw("All available keys");
                foreach (var key in keys)
                {
                    _output.WriteRaw(string.Format("[{0}] {1}", key.Identifier, (arg == "KEYS") ? key.PublicKey : key.Alias));
                }
                return;
            }

            if (arg == "ENDPOINTS")
            {
                var endpoints = _peerConfiguration.GetEndpoints().ToList();
                if (endpoints.Count == 0)
                {
                    _output.WriteRaw("No available endpoints");
                    return;
                }

                _output.WriteRaw("All available endpoints");
                for (int i=0; i < endpoints.Count; i++)
                {
                    _output.WriteRaw(string.Format("[{0}] {1}", i, endpoints[i]));
                }
                return;
            }
        }
    }
}
