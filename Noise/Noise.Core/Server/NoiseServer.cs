using Noise.Core.Abstraction;
using Noise.Core.Events;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Server
{
    public class NoiseServer : INoiseServer
    {
        private readonly IOutput _output;
        private readonly IPacketService _packetService;
        private readonly PeerConfiguration _peerConfiguration;

        private readonly bool _verboseMode;

        private TcpListener tcpListener;
        private bool _disposed = false;

        public event EventHandler<ClientDisconnectedEventsArgs> OnClientDisconnected;
        public event EventHandler<PacketPairReceivedEventsArgs> OnMessageReceived;
        public event EventHandler<PacketReceivedEventsArgs> OnPingReceived;
        public event EventHandler<PacketPairReceivedEventsArgs> OnDiscoveryReceived;

        private NoiseServer() { }
        public NoiseServer(IOutput output, IPacketService packetService, PeerConfiguration peerConfiguration)
        {
            OnPingReceived += PingEventHandler;
            OnDiscoveryReceived += DiscoveryEventHandler;
            OnMessageReceived += MessageEventHandler;
            OnClientDisconnected += ClientDisconnectedEventHandler;

            _output = output ??
                throw new ArgumentNullException(nameof(output));

            _packetService = packetService ??
                throw new ArgumentNullException(nameof(packetService));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _verboseMode = _peerConfiguration.VerboseMode;
        }

        private void MessageEventHandler(object sender, PacketPairReceivedEventsArgs e)
        {
            string endpoint = e.Client.Client.RemoteEndPoint.ToString();

            var (publicKey, message) = _packetService.RetriveMessagePacket(e.KeyPacket, e.CipherPacket, _peerConfiguration.PrivateKeyXml);

            if (publicKey is not null && message is not null)
            {
                string alias = _peerConfiguration.GetAliasByKey(publicKey);

                _output.WriteMessage(publicKey, message, endpoint, alias);
                return;
            }

            if (_verboseMode)
            {
                _output.WriteLog($"Message decryption from {endpoint} failed.");
            }
        }

        private void DiscoveryEventHandler(object sender, PacketPairReceivedEventsArgs e)
        {
            string endpoint = e.Client.Client.RemoteEndPoint.ToString();

            var (publicKeys, endpoints) = _packetService.RetriveDiscoveryPacket(e.KeyPacket, e.CipherPacket, _peerConfiguration.PrivateKeyXml);

            if (publicKeys is not null && endpoints is not null)
            {
                _peerConfiguration.InsertKeys(publicKeys);
                _peerConfiguration.InsertEndpoints(endpoints);

                if (_verboseMode)
                {
                    _output.WriteLog($"Discovered {publicKeys.Count()} identites and {endpoints.Count()} peers via {endpoint} peer.");
                    return;
                }
                
                _output.WriteLog("New peers discovered!");
                return;
            }

            if (_verboseMode)
            {
                _output.WriteLog($"Discovery decryption from {endpoint} failed.");
            }
        }

        private void PingEventHandler(object sender, PacketReceivedEventsArgs e)
        {
            _output.WritePing(e.Client.Client.RemoteEndPoint.ToString());
        }

        public void Dispose()
        {
            if (tcpListener is not null)
            {
                tcpListener.Stop();
                _disposed = true;
                tcpListener = null;
            }

            GC.SuppressFinalize(this);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            tcpListener = new TcpListener(IPAddress.Any, Constants.ProtocolPort);
            tcpListener.Start();

            cancellationToken.Register(tcpListener.Stop);

            while (!cancellationToken.IsCancellationRequested && !_disposed)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();

                    var clientTask = HandleClientConnection(client, cancellationToken)
                        .ContinueWith(callback => client.Dispose(), cancellationToken);
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    _output.WriteLog("The listening session is ended.");
                }
                catch (Exception ex)
                {
                    _output.WriteException("Error during communication resolving.", ex);
                }
            }
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task HandleClientConnection(TcpClient client, CancellationToken ct)
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Packet buffers
            IPacket keyPacket = null;
            IPacket cipherPacket = null;
            PacketType? cipherPacketType = null;

            while (client.Client.Connected && !ct.IsCancellationRequested && !_disposed)
            {
                try
                {
                    // Receive all the data from the network stream
                    var networkStream = client.GetStream();

                    var buffer = new byte[Constants.MaximalPacketBytesSize];
                    int bytes;

                    do
                    {
                        bytes = networkStream.Read(buffer, 0, buffer.Length);
                    } while (bytes == 0);

                    var result = new byte[bytes];
                    Array.Copy(buffer, 0, result, 0, bytes);

                    // Handle the stream as a packet after the data transmision is finished
                    var packet = Packet.Factory.FromBuffer(result);

                    // Select the correct action, depending on the packet type
                    switch (packet.Type)
                    {
                        case PacketType.PING:
                            OnPingReceived?.Invoke(this, new PacketReceivedEventsArgs
                            {
                                Client = client,
                                Packet = packet
                            });
                            break;

                        case PacketType.KEY:
                            keyPacket = packet;
                            break;

                        case PacketType.MESSAGE:
                            cipherPacket = packet;
                            cipherPacketType = packet.Type;
                            break;

                        case PacketType.DISCOVERY:
                            cipherPacket = packet;
                            cipherPacketType = packet.Type;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(packet), "Invalid packet type. The packet may be corrupted.");
                    }

                    // If ,,pair'' packets are obtained, raise the correct event
                    if (keyPacket is not null && cipherPacket is not null && cipherPacketType is not null)
                    {
                        var eventArgs = new PacketPairReceivedEventsArgs
                        {
                            Client = client,
                            CipherPacket = (Packet)cipherPacket,
                            KeyPacket = (Packet)keyPacket
                        };

                        if (cipherPacketType.Value == PacketType.MESSAGE) OnMessageReceived?.Invoke(this, eventArgs);
                        if (cipherPacketType.Value == PacketType.DISCOVERY) OnDiscoveryReceived?.Invoke(this, eventArgs);
                    }
                }
                catch
                {
                    if (!client.Client.Connected)
                    {
                        OnClientDisconnected?.Invoke(this, new ClientDisconnectedEventsArgs { Endpoint = client.Client.LocalEndPoint });
                        return;
                    }

                    throw;
                }
            }
        }

        private void ClientDisconnectedEventHandler(object sender, ClientDisconnectedEventsArgs e)
        {
            if (_verboseMode) _output.WriteLog($"{e.Endpoint} disonnected.");
        }
    }
}
