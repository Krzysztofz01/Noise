using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Core.Server.Events;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Server
{
    public class NoiseServer : INoiseServer
    {
        private readonly IOutput _output;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly NoiseServerConfiguration _noiseServerConfiguration;

        public event EventHandler<DiscoveryReceivedEventArgs> OnDiscoveryReceived;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<PeerConnectedEventArgs> OnPeerConnected;
        public event EventHandler<PeerDisconnectedEventArgs> OnPeerDisconnected;
        public event EventHandler<PingReceivedEventArgs> OnPingReceived;
        public event EventHandler<SignatureReceivedEventArgs> OnSignatureReceived;

        private ConcurrentDictionary<string, PeerMetadata> _peers;
        private ConcurrentDictionary<string, DateTime> _peersLastSeen;
        private ConcurrentDictionary<string, DateTime> _peersTimedout;

        private TcpListener _tcpListener;
        private bool _isListening;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        private NoiseServer() { }
        public NoiseServer(IOutput output, PeerConfiguration peerConfiguration, NoiseServerConfiguration noiseServerConfiguration)
        {
            _output = output ??
                throw new ArgumentNullException(nameof(output));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _noiseServerConfiguration = noiseServerConfiguration ??
                throw new ArgumentNullException(nameof(noiseServerConfiguration));

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
            throw new NotImplementedException();
        }

        private void SignatureReceivedEventHandler(object sender, SignatureReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PingReceivedEventHandler(object sender, PingReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PeerDisconnectedEventHandler(object sender, PeerDisconnectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DiscoveryReceivedEventHandler(object sender, DiscoveryReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _output.WriteLog("Starting up the server...");
            if (_isListening) throw new InvalidOperationException("The server is already running.");

            _tcpListener = new TcpListener(IPAddress.Any, Constants.ProtocolPort);

            ApplySocketSettings();

            _tcpListener.Start();
            _isListening = true;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            _output.WriteLog("Server started. Waiting for incoming connections.");
            return Task.Run(() => AcceptConnections(), _token);
        }

        public void Stop()
        {
            _output.WriteLog("Shuting down the sever...");
            if (!_isListening) throw new InvalidOperationException("The server is not running.");

            _isListening = false;
            _tcpListener.Stop();
            _tokenSource.Cancel();

            _output.WriteLog("Server stopped successful.");
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
                        PeerEndpoint = peerClient.Client.RemoteEndPoint
                    });

                    ApplySocketSettings(peerClient);

                    CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(peer.Token, _token);
                    Task unawaitedHandling = Task.Run(() => DataReceiver(peer), linkedTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    _isListening = false;
                    if (peer is not null) peer.Dispose();
                    return;
                }
                catch (OperationCanceledException)
                {
                    _isListening = false;
                    if (peer is not null) peer.Dispose();
                    return;
                }
                catch (ObjectDisposedException)
                {
                    if (peer != null) peer.Dispose();
                    continue;
                }
                catch (Exception ex)
                {
                    if (peer != null) peer.Dispose();
                    _output.WriteException("Exception while awaiting connections.", ex);
                    continue;
                }
            }

            _isListening = false;
        }

        private async Task DataReceiver(PeerMetadata peerMetadata)
        {
            throw new NotImplementedException();
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
            }
            catch (Exception ex)
            {
                _output.WriteException("Invalid server configuration.", ex);
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
                _output.WriteException("Invalid server configuration.", ex);
                throw;
            }
        }
    }
}
