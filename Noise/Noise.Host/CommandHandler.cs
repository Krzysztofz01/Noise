using Noise.Core.Abstraction;
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
                case "ALIAS": ExecuteAlias(args); return;
                case "INSERT": ExecuteInsert(args); return;
                case "HELP": ExecuteHelp(); return;
                case "SIGN": ExecuteSign(cancellationTokenSource); return;

                default: throw new InvalidOperationException("Invalid command.");
            }
        }

        private void ExecuteSign(CancellationTokenSource cts)
        {
            throw new NotImplementedException();
        }

        private Task ExecuteSend(string[] args, CancellationTokenSource cts)
        {
            throw new NotImplementedException();
        }

        private void ExecuteList(string[] args)
        {
            throw new NotImplementedException();
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

                var ordinalNumber = Convert.ToInt32(args.First());

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
            _outputMonitor.WriteRaw(Title.asciiTitle, false);

            _outputMonitor.WriteRaw($"{Environment.NewLine}v0.0.1");
            _outputMonitor.WriteRaw("https://github.com/Krzysztofz01/Noise");

            _outputMonitor.WriteRaw($"{Environment.NewLine}Available commands:");

            _outputMonitor.WriteRaw("EXIT - Close connections, save local data and exit.");
            _outputMonitor.WriteRaw("CLEAR - Clear the screen.");
            _outputMonitor.WriteRaw("LIST - List available peer keys, aliases or endpoints.");
            _outputMonitor.WriteRaw("SELECT - Select a peer to perform interactions.");
            _outputMonitor.WriteRaw("RESET - Reset selected peer.");
            _outputMonitor.WriteRaw("SEND - Send message to selected peer.");
            _outputMonitor.WriteRaw("SIGN - Send signature to selected peer.");
            _outputMonitor.WriteRaw("ALIAS - Set alias to certain peer.");
            _outputMonitor.WriteRaw("INSERT - Insert new peer key and optional alias or a endpoint.");
            _outputMonitor.WriteRaw("HELP - Show available commands.");
        }

        private void ExecuteClear()
        {
            _outputMonitor.Clear();
        }

        private void ExecuteExit(CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
