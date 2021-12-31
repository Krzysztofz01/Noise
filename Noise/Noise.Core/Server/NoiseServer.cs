using Noise.Core.Abstraction;
using Noise.Core.Events;
using Noise.Core.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Server
{
    public class NoiseServer : INoiseServer
    {
        private bool _disposed = false;

        private TcpListener tcpListener;

        public event EventHandler<ClientDisconnectedEventsArgs> OnClientDisconnected;
        public event EventHandler<PacketReceivedEventsArgs> OnMessageReceived;
        public event EventHandler<PacketReceivedEventsArgs> OnPingReceived;
        public event EventHandler<PacketReceivedEventsArgs> OnKeyReceived;
        public event EventHandler<PacketReceivedEventsArgs> OnDiscoveryReceived;

        public NoiseServer() { }

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

                    var clientTask = HandleClient(client, cancellationToken)
                        .ContinueWith(callback => client.Dispose(), cancellationToken);
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    Log("The listening session is ended.");
                }
                catch (Exception ex)
                {
                    Log("Error during communication resolving.", ex);
                }
            }
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task HandleClient(TcpClient client, CancellationToken ct)
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            while (client.Client.Connected && !ct.IsCancellationRequested && !_disposed)
            {
                try
                {
                    var networkStream = client.GetStream();

                    var buffer = new byte[Constants.MaximalPacketBytesSize];
                    int bytes;

                    do
                    {
                        bytes = networkStream.Read(buffer, 0, buffer.Length);
                    } while (bytes == 0);

                    var result = new byte[bytes];
                    Array.Copy(buffer, 0, result, 0, bytes);

                    var packet = Packet.Factory.FromBuffer(result);

                    var eventArgs = new PacketReceivedEventsArgs
                    {
                        Client = client,
                        Packet = packet
                    };

                    switch (packet.Type)
                    {
                        case PacketType.PING: OnPingReceived?.Invoke(this, eventArgs); break;
                        case PacketType.MESSAGE: OnMessageReceived?.Invoke(this, eventArgs); break;
                        case PacketType.KEY: OnKeyReceived?.Invoke(this, eventArgs); break;
                        case PacketType.DISCOVERY: OnDiscoveryReceived?.Invoke(this, eventArgs); break;

                        default: throw new ArgumentOutOfRangeException(nameof(packet), "Invalid packet type. The packet may be corrupted.");
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

        private static void Log(string log) => Console.WriteLine(log);
        private static void Log(string log, Exception ex) => Console.WriteLine($"{log} - {ex.Message}");
    }
}
