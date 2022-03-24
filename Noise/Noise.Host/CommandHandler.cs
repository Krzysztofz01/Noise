using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.Peer;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host
{
    internal class CommandHandler : ICommandHandler
    {
        private const int _publicKeyStripLength = 10;
        private const string _defaultPrompt = "> ";

        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;

        private RemotePeer selectedPeer = null;

        private CommandHandler() { }
        public CommandHandler(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));
        }

        public void Prefix()
        {
            if (selectedPeer is null)
            {
                _outputMonitor.WriteRaw(_defaultPrompt, false);
                return;
            }

            var peerName = selectedPeer.Alias != "Anonymous"
                ? selectedPeer.Alias
                : selectedPeer.PublicKey[.._publicKeyStripLength];

            _outputMonitor.WriteRaw(peerName, false);
        }

        public async Task Execute(string command, CancellationTokenSource cancellationTokenSource)
        {
            var commandArr = command.Trim().Split(" ");
            if (!commandArr.Any()) return;

            var type = commandArr.First().ToUpper();
            var args = commandArr.Skip(1).ToArray();

            switch (type)
            {
                case "EXIT": ExecuteExit(cancellationTokenSource); return;
                case "CLEAR": ExecuteClear(); return;
                case "LIST": ExecuteList(args); return;
                case "SELECT": ExecuteSelect(args); return;
                case "RESET": ExecuteReset(); return;
                case "SEND": await ExecuteSend(args, cancellationTokenSource); return;
                case ":": await ExecuteSend(args, cancellationTokenSource); return;
                case "ALIAS": ExecuteAlias(args); return;
                case "INSERT": ExecuteInsert(args); return;
                case "HELP": ExecuteHelp(); return;
                case "SIGN": await ExecuteSign(args, cancellationTokenSource); return;

                default: throw new CommandHandlerException("Invalid command. Use the HELP command for further information.");
            }
        }

        private INoiseClient CreateClient(string endpoint)
        {
            return new NoiseClient(endpoint, _outputMonitor, _peerConfiguration, new NoiseClientConfiguration
            {
                VerboseMode = _peerConfiguration.VerboseMode
            });
        }

        private async Task ExecuteSign(string[] args, CancellationTokenSource cts)
        {
            const string usage = "Usage: SIGN [overrite]";

            try
            {
                if (selectedPeer is null)
                    throw new CommandHandlerException("No peer selected. Use the SELECT command or check all commands using HELP.");

                bool overrite = args.Any(a => a.ToLower() == "overrite");

                if (selectedPeer.SendingSignature is not null && !overrite)
                    throw new CommandHandlerException($"This peer has a signature assigned.{Environment.NewLine}{usage}");

                foreach (var endpoint in _peerConfiguration.GetEndpoints())
                {
                    using var client = CreateClient(endpoint);
                    await client.SendSignature(selectedPeer.PublicKey, cts.Token);
                }
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private async Task ExecuteSend(string[] args, CancellationTokenSource cts)
        {
            const string usage = "Usage: SEND (or : for short) [message]";

            try
            {
                if (args.Length != 1)
                    throw new CommandHandlerException(usage);

                if (selectedPeer is null)
                    throw new CommandHandlerException("No peer selected. Use the SELECT command or check all commands using HELP.");

                var message = args.Single();

                foreach (var endpoint in _peerConfiguration.GetEndpoints())
                {
                    using var client = CreateClient(endpoint);
                    await client.SendMessage(selectedPeer.PublicKey, message, cts.Token);
                }

            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteList(string[] args)
        {
            const string usage = "Usage: LIST [full-public-key]";

            try
            {
                bool fullPublicKey = args.Any(a => a.ToLower() == "full-public-key");

                foreach (var peer in _peerConfiguration.GetPeers())
                {
                    string publicKey = fullPublicKey ? peer.PublicKey : peer.PublicKey[.._publicKeyStripLength];
                    _outputMonitor.WriteRaw($"[{peer.Identifier}] {peer.Alias} - {publicKey}", true);
                }

                _outputMonitor.LogInformation($"You can optionaly print the full public key.{Environment.NewLine}{usage}");

            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteReset()
        {
            selectedPeer = null;

            _outputMonitor.LogInformation("Selected peer reseted.");
        }

        private void ExecuteSelect(string[] args)
        {
            const string usage = "Usage: SELECT [peer_ordinal_number]";

            try
            {
                if (args.Length != 1)
                    throw new CommandHandlerException(usage);

                var ordinalNumber = Convert.ToInt32(args.Single());

                selectedPeer = _peerConfiguration.GetPeerByOrdinalNumberIdentifier(ordinalNumber);

                var peerName = selectedPeer.Alias != "Anonymous"
                    ? selectedPeer.Alias
                    : selectedPeer.PublicKey[.._publicKeyStripLength];

                _outputMonitor.LogInformation($"Peer: {peerName} selected.");
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteAlias(string[] args)
        {
            const string usage = "Usage: ALIAS [peer_ordinal_number] [value]";

            try
            {
                if (args.Length != 2)
                    throw new CommandHandlerException(usage);

                var ordinalNumber = Convert.ToInt32(args.First());
                var alias = args.Last();

                var peer = _peerConfiguration.GetPeerByOrdinalNumberIdentifier(ordinalNumber);
                _peerConfiguration.InsertAlias(peer.PublicKey, alias);

                _outputMonitor.LogInformation($"Alias: {alias} applied successful to peer: {peer.PublicKey}");
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteInsert(string[] args)
        {
            const string usage = "Usage: INSERT <peer/endpoint> [value]";

            try
            {
                if (args.Length != 2)
                    throw new CommandHandlerException(usage);

                var type = args.First().ToLower();
                var value = args.Last();

                if (type == "peer")
                {
                    _peerConfiguration.InsertPeer(value);
                    _outputMonitor.LogInformation("Peer with given public key added successful.");
                }

                if (type == "endpoint")
                {
                    _peerConfiguration.InsertEndpoint(value);
                    _outputMonitor.LogInformation("Given endpoint registered successful.");
                }

                throw new CommandHandlerException(usage);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteHelp()
        {
            ((OutputMonitor)_outputMonitor).WriteRaw(Title.asciiTitle, ConsoleColor.DarkGreen, false);

            ((OutputMonitor)_outputMonitor).WriteRaw($"{Environment.NewLine}v0.0.1", ConsoleColor.Green);
            ((OutputMonitor)_outputMonitor).WriteRaw("https://github.com/Krzysztofz01/Noise", ConsoleColor.Green);

            ((OutputMonitor)_outputMonitor).WriteRaw($"{Environment.NewLine}Available commands:", ConsoleColor.DarkYellow);

            ((OutputMonitor)_outputMonitor).WriteRaw("EXIT - Close connections, save local data and exit.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("CLEAR - Clear the screen.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("LIST - List available peer keys, aliases or endpoints.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("SELECT - Select a peer to perform interactions.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("RESET - Reset selected peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("SEND(:) - Send message to selected peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("SIGN - Send signature to selected peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("ALIAS - Set alias to certain peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("INSERT - Insert new peer key and optional alias or a endpoint.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("HELP - Show available commands.", ConsoleColor.Yellow);
        }

        private void ExecuteClear()
        {
            _outputMonitor.Clear();
        }

        private void ExecuteExit(CancellationTokenSource cancellationTokenSource)
        {
            _outputMonitor.LogInformation("Closing the Noise peer host...");
            cancellationTokenSource.Cancel();
        }
    }
}
