using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Host.Abstraction;
using Noise.Host.Exceptions;
using System;
using System.Collections.Generic;
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

        //private RemotePeer selectedPeer = null;
        private readonly SendingTarget _sendingTarget = new SendingTarget();

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
            if (!_sendingTarget.IsSelected())
            {
                _outputMonitor.WriteRaw(_defaultPrompt, false);
                return;
            }

            _outputMonitor.WriteRaw($"({_sendingTarget.GetTargetPrefix()}) {_defaultPrompt}", false);
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
                case "BLEACH": ExecuteBleach(); return;
                case "PING": await ExecutePing(args, cancellationTokenSource); return;
                case "ALIAS": ExecuteAlias(args); return;
                case "INSERT": ExecuteInsert(args); return;
                case "REMOVE": ExecuteRemove(args); return;
                case "HELP": ExecuteHelp(); return;
                case "SIGN": await ExecuteSign(args, cancellationTokenSource); return;
                case "INFO": ExecuteInfo(); return;
                case "DISCOVER": await ExecuteDiscover(cancellationTokenSource); return;

                default: throw new CommandHandlerException("Invalid command. Use the HELP command for further information.");
            }
        }

        private async Task ExecuteDiscover(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                foreach (var endpoint in _peerConfiguration.GetEndpoints(true))
                {
                    foreach (var peer in _peerConfiguration.GetPeers())
                    {
                        using var client = CreateClient(endpoint.Endpoint);
                        await client.SendDiscovery(peer.PublicKey, cancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteBleach()
        {
            try
            {
                if (!_peerConfiguration.Preferences.AllowPeerSignatureBleach)
                {
                    _outputMonitor.LogWarning("You can not bleach the signature with the AllowPeerSignatureBleach preference set to false");
                    return;
                }

                if (!_sendingTarget.IsSelected())
                    throw new CommandHandlerException("No peer selected. Use the SELECT command or check all commands using HELP.");

                if (_sendingTarget.IsGroup())
                    throw new CommandHandlerException("This operation can not be performed on a group of peers.");

                var selectedPeer = _sendingTarget.GetTarget();

                selectedPeer.SetReceivingSignature(null);

                // Fix for the: [Unknown] Identity prove unexpected mismatch
                // Reset only the receiving signature, beacuse the sending signature is changed during SIGN command
                //
                // selectedPeer.SetSendingSignature(null);

                ((OutputMonitor)_outputMonitor).WriteRaw("Peer bleached successful.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private async Task ExecutePing(string[] args, CancellationTokenSource cts)
        {
            const string usage = "Usage: PING [endpoint]";

            try
            {
                if (args.Length != 1)
                    throw new CommandHandlerException(usage);

                var endpoint = args.Single();

                using var client = CreateClient(endpoint);
                await client.SendPing(cts.Token);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteInfo()
        {
            ((OutputMonitor)_outputMonitor).WriteRaw("Local peer public key:", ConsoleColor.Green);
            ((OutputMonitor)_outputMonitor).WriteRaw(_peerConfiguration.Secrets.PublicKey, ConsoleColor.Yellow);

            _outputMonitor.WriteRaw(string.Empty);
            ((OutputMonitor)_outputMonitor).WriteRaw("Preferences:", ConsoleColor.Green);
            foreach (var option in _peerConfiguration.GetPreferences())
            {
                ((OutputMonitor)_outputMonitor).WriteRaw($"{option.Key}: {option.Value}", ConsoleColor.Yellow);
            }
        }

        public void Config(string[] args)
        {
            const string usage = "Usage: --config [option-name] [option-value]";

            try
            {
                var property = args.Skip(1).SkipLast(1).SingleOrDefault() ??
                    throw new CommandHandlerException(usage);

                var value = args.Skip(2).SingleOrDefault() ??
                    throw new CommandHandlerException(usage);

                if (!_peerConfiguration.ApplyPreference(property, value))
                    throw new CommandHandlerException($"Failed to apply: {value} to: {property} option.");

                if (PeerPreferences.IsDangerous(property))
                    _outputMonitor.LogWarning($"Changing the option: {property} is dangerous and is not recommended.");

                ((OutputMonitor)_outputMonitor).WriteRaw($"Successful applied: {value} to: {property} option.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private INoiseClient CreateClient(string endpoint)
        {
            return new NoiseClient(endpoint, _outputMonitor, _peerConfiguration, new NoiseClientConfiguration
            {
                VerboseMode = _peerConfiguration.Preferences.VerboseMode,
                StreamBufferSize = _peerConfiguration.Preferences.ClientStreamBufferSize,
                ConnectTimeoutMs = _peerConfiguration.Preferences.ClientConnectTimeoutMs,
                ReadTimeoutMs = _peerConfiguration.Preferences.ClientReadTimeoutMs,
                MaxConnectRetryCount = _peerConfiguration.Preferences.ClientMaxConnectRetryCount
            });
        }

        private async Task ExecuteSign(string[] args, CancellationTokenSource cts)
        {
            const string usage = "Usage: SIGN [overrite]";

            try
            {
                if (!_sendingTarget.IsSelected())
                    throw new CommandHandlerException("No peer selected. Use the SELECT command or check all commands using HELP.");

                if (_sendingTarget.IsGroup())
                    throw new CommandHandlerException("This operation can not be performed on a group of peers.");

                var selectedPeer = _sendingTarget.GetTarget();

                bool overrite = args.Any(a => a.ToLower() == "overrite");

                if (_peerConfiguration.IsSendingSignatureDefinedForPeer(selectedPeer.PublicKey) && !overrite)
                    throw new CommandHandlerException($"This peer has a signature assigned.{Environment.NewLine}{usage}");

                foreach (var endpoint in _peerConfiguration.GetEndpoints(true))
                {
                    using var client = CreateClient(endpoint.Endpoint);
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
                if (args.Length == 0)
                    throw new CommandHandlerException(usage);

                if (!_sendingTarget.IsSelected())
                    throw new CommandHandlerException("No peer selected. Use the SELECT command or check all commands using HELP.");

                var message = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(message))
                    throw new CommandHandlerException(usage);

                var messageTargets = _sendingTarget.IsGroup()
                    ? _sendingTarget.GetTargets()
                    : new List<RemotePeer> { _sendingTarget.GetTarget() };

                foreach (var target in messageTargets)
                {
                    foreach (var endpoint in _peerConfiguration.GetEndpoints(true))
                    {
                        using var client = CreateClient(endpoint.Endpoint);
                        await client.SendMessage(target.PublicKey, message, cts.Token);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private void ExecuteList(string[] args)
        {
            const string usage = "Usage: LIST <peer/endpoint> [full-public-key]";

            try
            {
                if (args.Length != 1)
                    throw new CommandHandlerException(usage);

                var type = args.First().ToLower();
                var fullPublicKey = args.Skip(1).Any(a => a.ToLower() == "full-public-key");

                if (type == "peer")
                {
                    foreach (var peer in _peerConfiguration.GetPeers())
                    {
                        string publicKey = fullPublicKey ? peer.PublicKey : peer.PublicKey[.._publicKeyStripLength];
                        string signingDetails = GetSignatureDetails(peer.PublicKey);

                        _outputMonitor.WriteRaw($"[{peer.Identifier}] {peer.Alias} ({signingDetails}) - {publicKey}", true);
                    }

                    return;
                }

                if (type == "endpoint")
                {
                    foreach (var endpoint in _peerConfiguration.GetEndpoints(false).OrderBy(e => e.IsConnected))
                    {
                        var status = endpoint.IsConnected ? "Online" : "Offline";
                        _outputMonitor.WriteRaw($"{endpoint.Endpoint} [{status}]", true);
                    }

                    return;
                }

                throw new CommandHandlerException(usage);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        private string GetSignatureDetails(string publicKey)
        {
            var receiving = _peerConfiguration.IsReceivingSignatureDefinedForPeer(publicKey);
            var sending = _peerConfiguration.IsSendingSignatureDefinedForPeer(publicKey);

            if (receiving && sending) return "Double-signed";
            if (receiving && !sending) return "Receiving-signed";
            if (!receiving && sending) return "Sending-signed";
            return "Unsigned";
        }

        private void ExecuteReset()
        {
            _sendingTarget.Reset();

            _outputMonitor.LogInformation("Selected peer reseted.");
        }

        private void ExecuteSelect(string[] args)
        {
            const string usage = "Usage: SELECT [peer_ordinal_number]";

            try
            {
                if (args.Length > 1)
                    throw new CommandHandlerException(usage);

                if (args.Length == 0)
                {
                    if (!_sendingTarget.IsGroup())
                    {
                        _outputMonitor.LogInformation("Use this command to list target group. A ssingle peer is currently selected.");
                        return;
                    }

                    var peerNames = _sendingTarget.GetTargets()
                        .Select(p => p.Alias != "Anonymous" ? p.Alias : p.PublicKey[.._publicKeyStripLength]);

                    foreach (var name in peerNames)
                        _outputMonitor.WriteRaw($"- {name}");

                    return;
                }

                var ordinalNumber = Convert.ToInt32(args.Single());

                var selectedPeer = _peerConfiguration.GetPeerByOrdinalNumberIdentifier(ordinalNumber);

                _sendingTarget.AddPeer(selectedPeer);

                var peerName = selectedPeer.Alias != "Anonymous"
                    ? selectedPeer.Alias
                    : selectedPeer.PublicKey[.._publicKeyStripLength];

                if (_sendingTarget.IsGroup())
                {
                    var groupCount = _sendingTarget.GetTargets().Count();
                    _outputMonitor.LogInformation($"Peer: {peerName} selected. Total group of {groupCount} peers.");
                }
                else
                {
                    _outputMonitor.LogInformation($"Peer: {peerName} selected.");
                }
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
                    return;
                }

                if (type == "endpoint")
                {
                    _peerConfiguration.InsertEndpoint(value);
                    _outputMonitor.LogInformation("Given endpoint registered successful.");
                    return;
                }

                throw new CommandHandlerException(usage);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerException(ex.Message);
            }
        }

        public void ExecuteRemove(string[] args)
        {
            const string usage = "Usage: REMOVE <peer/endpoint> [peer_ordinal_number/endpoint]";

            try
            {
                if (args.Length != 2)
                    throw new CommandHandlerException(usage);

                var type = args.First().ToLower();
                var value = args.Last();

                if (type == "peer")
                {
                    var ordinalNumber = Convert.ToInt32(value);
                    var peer = _peerConfiguration.GetPeerByOrdinalNumberIdentifier(ordinalNumber);

                    if (_sendingTarget.IsSelected())
                    {
                        //TODO: Test if the expression eval is working the same way like in C++
                        if (_sendingTarget.IsGroup() && _sendingTarget.GetTargets().Any(p => p.PublicKey == peer.PublicKey))
                        {
                            _sendingTarget.Reset();
                            _outputMonitor.LogInformation("Selected peer is no longer available.");
                        }

                        //TODO: Test if the expression eval is working the same way like in C++
                        if (!_sendingTarget.IsGroup() && _sendingTarget.GetTarget().PublicKey == peer.PublicKey)
                        {
                            _sendingTarget.Reset();
                            _outputMonitor.LogInformation("Selected peer is no longer available.");
                        }
                    }

                    _peerConfiguration.RemovePeer(peer.PublicKey);
                    _outputMonitor.LogInformation("Peer with given public key removed successful.");
                    return;
                }

                if (type == "endpoint")
                {
                    _peerConfiguration.RemoveEndpoint(value);
                    _outputMonitor.LogInformation("Given endpoint removed successful.");
                    return;
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
            ((OutputMonitor)_outputMonitor).WriteRaw(Title.AsciiTitle, ConsoleColor.DarkGreen, false);

            ((OutputMonitor)_outputMonitor).WriteRaw($"{Environment.NewLine}{Constants.Version}", ConsoleColor.Green);
            ((OutputMonitor)_outputMonitor).WriteRaw("https://github.com/Krzysztofz01/Noise", ConsoleColor.Green);

            ((OutputMonitor)_outputMonitor).WriteRaw($"{Environment.NewLine}Available commands:", ConsoleColor.DarkYellow);

            ((OutputMonitor)_outputMonitor).WriteRaw("EXIT - Close connections, save local data and exit.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("CLEAR - Clear the screen.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("LIST - List available peer keys, aliases or endpoints.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("SELECT - Select a peer to perform interactions.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("RESET - Reset selected peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("SEND(:) - Send message to selected peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("SIGN - Send signature to selected peer.", ConsoleColor.Yellow);

            if (_peerConfiguration.Preferences.AllowPeerSignatureBleach)
            {
                ((OutputMonitor)_outputMonitor).WriteRaw("BLEACH - Reset the receiving signature related to the selected peer.", ConsoleColor.Yellow);
            }
            
            ((OutputMonitor)_outputMonitor).WriteRaw("PING - Send a ping packet to a certain endpoint.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("ALIAS - Set alias to certain peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("INSERT - Insert new peer key and optional alias or a endpoint.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("REMOVE - Remove a given peer key or a endpoint.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("HELP - Show available commands.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("INFO - Print information about local peer.", ConsoleColor.Yellow);
            ((OutputMonitor)_outputMonitor).WriteRaw("DISCOVER - Broadcast discovery packets to the network.", ConsoleColor.Yellow);
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
