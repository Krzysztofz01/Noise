using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Host.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host
{
    public class CommandHandler : ICommandHandler
    {
        private const string _defaultPrefix = "> ";

        private int? selectedIdentifier = null;

        private readonly PeerConfiguration _peerConfiguration;
        private readonly IOutput _output;
        private readonly IPacketService _packetService;
        private readonly INoiseClient _noiseClient;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CommandHandler(PeerConfiguration peerConfiguration, IOutput output, IPacketService packetService, INoiseClient noiseClient, CancellationTokenSource cancellationTokenSource)
        {
            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _output = output ??
                throw new ArgumentNullException(nameof(output));

            _packetService = packetService ??
                throw new ArgumentNullException(nameof(packetService));

            _noiseClient = noiseClient ??
                throw new ArgumentNullException(nameof(noiseClient));

            _cancellationTokenSource = cancellationTokenSource ??
                throw new ArgumentNullException(nameof(cancellationTokenSource));
        }

        public void Prefix()
        {
            if (selectedIdentifier is null)
            {
                _output.WriteRaw(_defaultPrefix, false);
                return;
            }

            var peer = _peerConfiguration.GetPeerByIdentifier(selectedIdentifier.Value);
            var displayName = (peer.Alias != "Anonymous") ? peer.Alias : ShortenPublicKey(peer.PublicKey);

            _output.WriteRaw(string.Format("({0}) {1}", displayName, _defaultPrefix), false);
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
                case "SELECT": ExecuteSelect(args); return;
                case "RESET": ExecuteReset(); return;
                case "SEND": await ExecuteSend(args); return;
                case "ALIAS": ExecuteAlias(args); return;
                case "INSERT": ExecuteInsert(args); return;
                case "HELP": ExecuteHelp(); return;

                default: throw new InvalidOperationException("Invalid command.");
            }
        }

        private string ShortenPublicKey(string publicKey)
        {
            return $"{publicKey.Substring(0, 9)}...";
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
                    Console.ForegroundColor = ConsoleColor.Green;
                    _output.WriteRaw(string.Format("[{0}] ", key.Identifier), false);

                    Console.ResetColor();
                    _output.WriteRaw((arg == "KEYS") ? key.PublicKey : key.Alias);
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
                    Console.ForegroundColor = ConsoleColor.Green;
                    _output.WriteRaw(string.Format("[{0}] ", i), false);

                    Console.ResetColor();
                    _output.WriteRaw(endpoints[i]);
                }
                return;
            }

            _output.WriteRaw(usage);
        }

        private void ExecuteSelect(string[] args)
        {
            string usage = "Command usage: select <id, alias> <value>";

            if (args.Count() != 2)
            {
                _output.WriteRaw(usage);
                return;
            }

            string selector = args.First().ToUpper();
            string value = args.Last();

            try
            {
                if (selector == "ID")
                {
                    selectedIdentifier = _peerConfiguration.GetPeerByIdentifier(Convert.ToInt32(value)).Identifier;
                    return;
                }

                if (selector == "ALIAS")
                {
                    selectedIdentifier = _peerConfiguration.GetPeerByAlias(value).Identifier;
                    return;
                }

                _output.WriteRaw(usage);
            }
            catch (Exception ex)
            {
                _output.WriteException("Peer not found.", ex);
                return;
            }
        }

        private void ExecuteReset()
        {
            selectedIdentifier = null;
        }

        private async Task ExecuteSend(string[] args)
        {
            try
            {
                var receiverPeer = _peerConfiguration.GetPeerByIdentifier(selectedIdentifier.Value);

                var message = new StringBuilder(args.First());
                foreach (var arg in args.Skip(1))
                {
                    message.Append(' ');
                    message.Append(arg);
                }

                var appendedMessage = message.ToString();

                Console.ForegroundColor = ConsoleColor.Green;
                _output.WriteRaw(string.Format("(@You): {0}//local", appendedMessage));
                Console.ResetColor();

                var receiverPublicKey = receiverPeer.PublicKey;
                var (keyPacket, messagePacket) = _packetService.CreateMessagePackets(_peerConfiguration.PublicKey, receiverPublicKey, message.ToString());

                await _noiseClient.SendPacketsAsync(new List<IPacket> { keyPacket, messagePacket });
            }
            catch (Exception ex)
            {
                _output.WriteException("Message sending failure.", ex);
                return;
            }
        }

        private void ExecuteAlias(string[] args)
        {
            string usage = "Command usage: alias <id> <value>";

            if (args.Count() != 2)
            {
                _output.WriteRaw(usage);
                return;
            }

            int id = Convert.ToInt32(args.First());
            string alias = args.Last();

            try
            {
                var peer = _peerConfiguration.GetPeerByIdentifier(id);

                _peerConfiguration.InsertAlias(peer.PublicKey, alias);
            }
            catch (Exception ex)
            {
                _output.WriteException("Peer not found.", ex);
                return;
            }
        }

        private void ExecuteInsert(string[] args)
        {
            string usage = "Command usage: insert <peer/endpoint> <value - key or endpoint> <value - alias?>";

            var arguments = args.ToList();

            if (arguments.Count < 2)
            {
                _output.WriteRaw(usage);
                return;
            }

            if (arguments.First().ToUpper() == "PEER")
            {
                try
                {
                    string key = arguments[1];

                    if (arguments.Count > 2)
                    {
                        _peerConfiguration.InsertKey(key, arguments.Last());
                        return;
                    }

                    _peerConfiguration.InsertKey(key);
                }
                catch (Exception ex)
                {
                    _output.WriteException("Invalid key format.", ex);
                    return;
                }

                return;
            }

            if (arguments.First().ToUpper() == "ENDPOINT")
            {
                try
                {
                    string endpoint = arguments[1];

                    _peerConfiguration.InsertEndpoints(new List<string> { endpoint });
                }
                catch (Exception ex)
                {
                    _output.WriteException("Invalid endpoint format.", ex);
                    return;
                }

                return;
            }

            _output.WriteRaw(usage);
            return;
        }

        private void ExecuteHelp()
        {
            _output.WriteRaw(Title.asciiTitle, false);

            _output.WriteRaw($"{Environment.NewLine}v1.0.0");
            _output.WriteRaw("https://github.com/Krzysztofz01/Noise");

            _output.WriteRaw($"{Environment.NewLine}Available commands:");

            _output.WriteRaw("EXIT - Close connections, save local data and exit.");
            _output.WriteRaw("CLEAR - Clear the screen.");
            _output.WriteRaw("LIST - List available peer keys, aliases or endpoints.");
            _output.WriteRaw("SELECT - Select a peer to perform interactions.");
            _output.WriteRaw("RESET - Reset selected peer.");
            _output.WriteRaw("SEND - Send message to selected peer.");
            _output.WriteRaw("ALIAS - Set alias to certain peer.");
            _output.WriteRaw("INSERT - Insert new peer key and optional alias or a endpoint.");
            _output.WriteRaw("HELP - Show available commands.");
        }
    }
}
