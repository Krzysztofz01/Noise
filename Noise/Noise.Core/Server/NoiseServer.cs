using Noise.Core.Abstraction;
using Noise.Core.Exceptions;
using Noise.Core.Extensions;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Core.Server.Events;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Server
{
    public class NoiseServer : INoiseServer
    {
        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly NoiseServerConfiguration _noiseServerConfiguration;

        public event EventHandler<DiscoveryReceivedEventArgs> OnDiscoveryReceived;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<PeerConnectedEventArgs> OnPeerConnected;
        public event EventHandler<PeerDisconnectedEventArgs> OnPeerDisconnected;
        public event EventHandler<PingReceivedEventArgs> OnPingReceived;
        public event EventHandler<SignatureReceivedEventArgs> OnSignatureReceived;

        private readonly ConcurrentDictionary<string, PeerMetadata> _peers;
        private readonly ConcurrentDictionary<string, DateTime> _peersLastSeen;
        private readonly ConcurrentDictionary<string, DateTime> _peersTimedout;

        private TcpListener _tcpListener;
        private bool _isListening;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        private NoiseServer() { }
        public NoiseServer(IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration, NoiseServerConfiguration noiseServerConfiguration = null)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _noiseServerConfiguration = noiseServerConfiguration ??
                new NoiseServerConfiguration();

            OnDiscoveryReceived += DiscoveryReceivedEventHandler;
            OnMessageReceived += MessageReceivedEventHandler;
            OnPeerConnected += PeerConnectedEventHandler;
            OnPeerDisconnected += PeerDisconnectedEventHandler;
            OnPingReceived += PingReceivedEventHandler;
            OnSignatureReceived += SignatureReceivedEventHandler;

            _peers = new ConcurrentDictionary<string, PeerMetadata>();
            _peersLastSeen = new ConcurrentDictionary<string, DateTime>();
            _peersTimedout = new ConcurrentDictionary<string, DateTime>();
        }

        private void PeerConnectedEventHandler(object sender, PeerConnectedEventArgs e)
        {
            var senderEndpoint = e.PeerEndpoint;
            LogVerbose($"Peer with endpoint: {senderEndpoint} disconnected.");

            bool isEndpointKnown = _peerConfiguration.IsEndpointKnown(senderEndpoint);

            if (_peerConfiguration.Preferences.AcceptUnpromptedConnectionEndpoints && !isEndpointKnown)
                _peerConfiguration.InsertEndpoint(senderEndpoint);

            if (_peerConfiguration.Preferences.UseEndpointAttemptFilter && isEndpointKnown)
                _peerConfiguration.SetEndpointAsConnected(senderEndpoint);
        }

        private void SignatureReceivedEventHandler(object sender, SignatureReceivedEventArgs e)
        {
            try
            {
                var senderEndpoint = e.PeerEndpoint;
                LogVerbose($"Server received message packets from peer: {senderEndpoint}");

                var bufferQueue = PacketBufferQueueBuilder
                    .Create()
                    .InsertBuffer(e.PacketBufferQueue)
                    .Build();

                var keyBuffer = bufferQueue.Dequeue();
                var signatureBuffer = bufferQueue.Dequeue();

                var packetHandlingService = new PacketHandlingService();
                var (signature, senderPublicKey, certification) = packetHandlingService.ReceiveIdentityProve(keyBuffer, signatureBuffer, _peerConfiguration.Secrets.PrivateKey);

                if (_peerConfiguration.Preferences.IndependentMediumCertification is not null)
                {
                    if (_peerConfiguration.Preferences.IndependentMediumCertification != certification)
                    {
                        LogVerbose("Invalid independent medium certification configuration.");
                        return;
                    }
                }

                if (_peerConfiguration.HasPeerAssignedSignature(senderPublicKey))
                {
                    LogVerbose("Given peer has already asigned a signature. Someone might try to spoof another peer.");
                    return;
                }

                LogVerbose($"Signature received successful.");

                _peerConfiguration.GetPeerByPublicKey(senderPublicKey).SetReceivingSignature(signature);

                LogVerbose("Received signature applied successful.");
            }
            catch (PacketRejectedException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                LogVerbose($"Unexpected exception while receiving signature packets. {ex.Message}");
            }
        }

        private void PingReceivedEventHandler(object sender, PingReceivedEventArgs e)
        {
            var senderEndpoint = e.PeerEndpoint;
            LogVerbose($"Server received ping packets from peer: {senderEndpoint}");

            _outputMonitor.WriteIncomingPing(senderEndpoint);
        }

        private void PeerDisconnectedEventHandler(object sender, PeerDisconnectedEventArgs e)
        {
            var senderEndpoint = e.PeerEndpoint;
            LogVerbose($"Peer with endpoint: {senderEndpoint} disconnected.");
        }

        private void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var senderEndpoint = e.PeerEndpoint;
                LogVerbose($"Server received message packets from peer: {senderEndpoint}");

                var bufferQueue = PacketBufferQueueBuilder
                    .Create()
                    .InsertBuffer(e.PacketBufferQueue)
                    .Build();

                var keyBuffer = bufferQueue.Dequeue();
                var messageBuffer = bufferQueue.Dequeue();

                var packetHandlingService = new PacketHandlingService();
                var (senderIdentityProve, message) = packetHandlingService.ReceiveMessage(keyBuffer, messageBuffer, _peerConfiguration.Secrets.PrivateKey);

                if (!_peerConfiguration.IsReceivingSignatureValid(senderIdentityProve))
                {
                    throw new PacketRejectedException(PacketRejectionReason.INVALID_IDENTITY_PROVE);
                }

                var senderPeer = _peerConfiguration.GetPeerByReceivingSignature(senderIdentityProve);

                LogVerbose($"Message received successful.");
                _outputMonitor.WriteIncomingMessage(
                    senderPeer.PublicKey,
                    senderPeer.Alias,
                    senderEndpoint,
                    message);
            }
            catch (PacketRejectedException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                LogVerbose($"Unexpected exception while receiving message packets. {ex.Message}");
            }
        }

        private void DiscoveryReceivedEventHandler(object sender, DiscoveryReceivedEventArgs e)
        {
            try
            {
                var senderEndpoint = e.PeerEndpoint;
                LogVerbose($"Server received discovery packets from peer: {senderEndpoint}");

                var bufferQueue = PacketBufferQueueBuilder
                    .Create()
                    .InsertBuffer(e.PacketBufferQueue)
                    .Build();

                var keyBuffer = bufferQueue.Dequeue();
                var discoveryBuffer = bufferQueue.Dequeue();

                var packetHandlingService = new PacketHandlingService();
                var (endpoints, publicKeys, senderIdentityProve) = packetHandlingService.ReceiveDiscoveryCollections(keyBuffer, discoveryBuffer, _peerConfiguration.Secrets.PrivateKey);
            
                if (!_peerConfiguration.IsReceivingSignatureValid(senderIdentityProve))
                {
                    throw new PacketRejectedException(PacketRejectionReason.INVALID_IDENTITY_PROVE);
                }

                var senderPeer = _peerConfiguration.GetPeerByReceivingSignature(senderIdentityProve);
                LogVerbose($"Discovery peer: {senderPeer.PublicKey}");

                int endpointCount = _peerConfiguration.GetEndpoints().Count();
                int peerCount = _peerConfiguration.GetPeers().Count();

                if (_peerConfiguration.Preferences.AcceptPublicKeysViaDiscovery)
                {
                    foreach (var publicKey in publicKeys)
                    {
                        _peerConfiguration.InsertPeer(publicKey);
                    }

                    int discoveredPeers = _peerConfiguration.GetPeers().Count() - peerCount;
                    LogVerbose($"Discovered {discoveredPeers} peer public keys.");
                }

                foreach (var endpoint in endpoints) _peerConfiguration.InsertEndpoint(endpoint);

                int discoveredEndpoints = _peerConfiguration.GetEndpoints().Count() - endpointCount;
                LogVerbose($"Discovered {discoveredEndpoints} endpoints.");
            }
            catch (PacketRejectedException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                LogVerbose($"Unexpected exception while receiving message packets. {ex.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                LogVerbose("Disposing the noise server.");

                try
                {
                    if (_peers is not null && !_peers.IsEmpty)
                    {
                        foreach (var peer in _peers)
                        {
                            peer.Value.Dispose();
                            LogVerbose($"Disposing connection with: {peer.Key}");
                        }
                    }

                    if (_tokenSource is not null)
                    {
                        if (!_tokenSource.IsCancellationRequested)
                            _tokenSource.Cancel();

                        _tokenSource.Dispose();
                    }

                    if (_tcpListener is not null && _tcpListener.Server is not null)
                    {
                        _tcpListener.Server.Close();
                        _tcpListener.Server.Dispose();
                    }

                    if (_tcpListener is not null)
                    {
                        _tcpListener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    _outputMonitor.LogError($"Some errors occured while disposing the server.", ex);
                }

                _isListening = false;

                LogVerbose("Noise server disposed.");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            LogVerbose("Starting up the noise server.");
            if (_isListening) throw new InvalidOperationException("The server is already running.");

            _tcpListener = new TcpListener(IPAddress.Any, Constants.ProtocolPort);

            ApplySocketSettings();

            _tcpListener.Start();
            _isListening = true;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            LogVerbose("Noise server started. Waiting for incoming connections.");
            return Task.Run(() => AcceptConnections(), _token);
        }

        public void Stop()
        {
            LogVerbose("Shuting down the noise sever.");
            if (!_isListening) throw new InvalidOperationException("The server is not running.");

            _isListening = false;
            _tcpListener.Stop();
            _tokenSource.Cancel();

            LogVerbose("Noise server stopped successful.");
        }

        private async Task AcceptConnections()
        {
            while (!_token.IsCancellationRequested)
            {
                PeerMetadata peer = null;

                try
                {
                    var peerClient = await _tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    string peerEndpoint = peerClient.Client.RemoteEndPoint.ToString();

                    peer = new PeerMetadata(peerClient);

                    _peers.TryAdd(peerEndpoint, peer);
                    _peersLastSeen.TryAdd(peerEndpoint, DateTime.Now);

                    OnPeerConnected?.Invoke(this, new PeerConnectedEventArgs
                    {
                        PeerEndpoint = peerEndpoint
                    });

                    ApplySocketSettings(peerClient);

                    CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(peer.Token, _token);
                    Task unawaitedHandling = Task.Run(() => DataReceiver(peer), linkedTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    LogVerbose("Connection accept task canceled.");

                    _isListening = false;
                    if (peer is not null) peer.Dispose();
                    return;
                }
                catch (OperationCanceledException)
                {
                    LogVerbose("Connection accept operation canceled.");

                    _isListening = false;
                    if (peer is not null) peer.Dispose();
                    return;
                }
                catch (ObjectDisposedException)
                {
                    LogVerbose("Server object disposed on accepting connection.");

                    if (peer is not null) peer.Dispose();
                    continue;
                }
                catch (Exception ex)
                {
                    if (_isListening && !_token.IsCancellationRequested)
                    {
                        LogVerbose($"Unexpected exception while awaiting connections. {ex.Message}");
                    }

                    if (peer is not null) peer.Dispose();
                    continue;
                }
            }

            _isListening = false;
        }

        private async Task DataReceiver(PeerMetadata peer)
        {
            string ipPort = peer.IpPort;
            LogVerbose($"Receiving data from: ${ipPort}");

            CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_token, peer.Token);

            while (true)
            {
                try
                {
                    if (!IsPeerConnected(peer.TcpClient))
                    {
                        LogVerbose($"Peer from endpoint: {ipPort} disconnected.");
                        break;
                    }

                    if (peer.Token.IsCancellationRequested)
                    {
                        LogVerbose($"Connection for peer from: {ipPort} canceled.");
                        break;
                    }

                    byte[] dataBuffer = await DataReadAsync(peer, linkedCts.Token).ConfigureAwait(false);
                    if (dataBuffer == null)
                    {
                        await Task.Delay(10, linkedCts.Token).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        Task unawaited = Task.Run(() =>
                        {
                            switch (PeekPacketType(dataBuffer))
                            {
                                case PacketType.MESSAGE: OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(dataBuffer, ipPort)); break;
                                case PacketType.DISCOVERY: OnDiscoveryReceived?.Invoke(this, new DiscoveryReceivedEventArgs(dataBuffer, ipPort)); break;
                                case PacketType.SIGNATURE: OnSignatureReceived?.Invoke(this, new SignatureReceivedEventArgs(dataBuffer, ipPort)); break;
                                case PacketType.PING: OnPingReceived?.Invoke(this, new PingReceivedEventArgs(ipPort)); break;

                                default: throw new ArgumentException("Invalid packet type. No handler defined.");
                            }
                        }, linkedCts.Token);
                        UpdatePeerLastSeen(peer.IpPort);
                    }
                }
                catch (IOException)
                {
                    LogVerbose($"Data receive for peer: {ipPort} canceled. Peer disconnected.");
                }
                catch (SocketException)
                {
                    LogVerbose($"Data receive for peer: {ipPort} canceled. Peer disconnected.");
                }
                catch (TaskCanceledException)
                {
                    LogVerbose($"Data receive for peer: {ipPort} canceled. Task canceled.");
                }
                catch (ObjectDisposedException)
                {
                    LogVerbose($"Data receive for peer: {ipPort} canceled. Task disposed.");
                }
                catch (Exception ex)
                {
                    LogVerbose($"Unexpected peer data receive failure. {ex.Message}");
                    break;
                }
            }

            LogVerbose($"Data receive terminated for peer: {ipPort}");

            if (_peersTimedout.ContainsKey(peer.IpPort))
            {
                OnPeerDisconnected?.Invoke(this, new PeerDisconnectedEventArgs
                {
                    PeerDisconnectReason = PeerDisconnectReason.Timeout,
                    PeerEndpoint = ipPort
                });
            }
            else
            {
                OnPeerDisconnected?.Invoke(this, new PeerDisconnectedEventArgs
                {
                    PeerDisconnectReason = PeerDisconnectReason.Normal,
                    PeerEndpoint = ipPort
                });
            }

            _peers.TryRemove(ipPort, out _);
            _peersLastSeen.TryRemove(ipPort, out _);
            _peersTimedout.TryRemove(ipPort, out _);

            if (peer is not null) peer.Dispose();
        }

        private void UpdatePeerLastSeen(string ipPort)
        {
            if (_peersLastSeen.ContainsKey(ipPort))
            {
                _peersLastSeen.TryRemove(ipPort, out _);
            }

            _peersLastSeen.TryAdd(ipPort, DateTime.Now);
        }

        private async Task<byte[]> DataReadAsync(PeerMetadata peer, CancellationToken token)
        {
            var buffer = new byte[Constants.MaximalPacketSize];
            int read = 0;

            using var memoryStream = new MemoryStream();
            while (true)
            {
                read = await peer.NetworkStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);

                if (read > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, read, token).ConfigureAwait(false);
                    return memoryStream.ToArray();
                }
                else
                {
                    LogVerbose("Read buffer not pupulated with network stream data.");
                    throw new SocketException();
                }
            }
        }

        private bool IsPeerConnected(TcpClient tcpClient)
        {
            if (!tcpClient.Connected) return false;

            if (tcpClient.Client.Poll(0, SelectMode.SelectWrite) && (!tcpClient.Client.Poll(0, SelectMode.SelectError)))
            {
                var buffer = new byte[1];
                if (tcpClient.Client.Receive(buffer, SocketFlags.Peek) == 0) return false;
                return true;
            }
            return false;
        }

        private void ApplySocketSettings()
        {
            try
            {
                if (_noiseServerConfiguration.EnableKeepAlive)
                {
                    _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    _tcpListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _noiseServerConfiguration.KeepAliveTime);
                    _tcpListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, _noiseServerConfiguration.KeepAliveInterval);
                    _tcpListener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, _noiseServerConfiguration.KeepAliveRetryCount);
                }

                if (OperatingSystem.IsWindows())
                {
                    if (_noiseServerConfiguration.EnableNatTraversal)
                        LogVerbose("Appling Windows system specific setting: EnableNatTraversal");

                    _tcpListener.AllowNatTraversal(_noiseServerConfiguration.EnableNatTraversal);
                }
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError("Invalid server configuration.", ex);
                throw;
            }
        }

        private void ApplySocketSettings(TcpClient tcpClient)
        {
            try
            {
                if (_noiseServerConfiguration.EnableKeepAlive)
                {
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _noiseServerConfiguration.KeepAliveTime);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, _noiseServerConfiguration.KeepAliveInterval);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, _noiseServerConfiguration.KeepAliveRetryCount);
                }
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError("Invalid server configuration.", ex);
                throw;
            }
        }

        private static PacketType PeekPacketType(byte[] packetBuffer)
        {
            if (packetBuffer.Length < Constants.PacketBaseSize)
                throw new ArgumentException("Invalid buffer size, the packet may be corrupted.");

            if (PacketBufferQueueBuilder.IsBufferQueue(packetBuffer))
            {
                return (PacketType)PacketBufferQueueBuilder
                    .Create().InsertBuffer(packetBuffer).Build()
                    .Skip(1).First()
                    .ToInt32(sizeof(Int32));
            }

            return (PacketType)packetBuffer.ToInt32(sizeof(Int32));
        }

        private void LogVerbose(string message)
        {
            if (_noiseServerConfiguration.VerboseMode)
                _outputMonitor.LogInformation(message);
        }
    }
}
