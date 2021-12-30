using Noise.Core.Abstraction;
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
        private bool verboseMode = false;
        private bool disposed = false;

        private TcpListener tcpListener;

        public NoiseServer() { }

        public void Dispose()
        {
            if (tcpListener is not null)
            {
                tcpListener.Stop();
                disposed = true;
                tcpListener = null;
            }

            GC.SuppressFinalize(this);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            tcpListener = new TcpListener(IPAddress.Any, Constants.ProtocolPort);
            tcpListener.Start();

            cancellationToken.Register(tcpListener.Stop);

            while (!cancellationToken.IsCancellationRequested && !disposed)
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

        private async Task HandleClient(TcpClient client, CancellationToken ct)
        {
            while (client.Client.Connected && !ct.IsCancellationRequested && !disposed)
            {
                IPacket packet;
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

                    packet = Packet.Factory.FromBuffer(result);
                }
                catch
                {
                    if (!client.Client.Connected)
                    {
                        //TODO: OnClientDisconnect handle
                    }
                    else
                    {
                        throw;
                    }
                }

                //TODO: Handle packet, packet handler service
            }
        }

        private void Log(string log) => Console.WriteLine(log);
        private void Log(string log, Exception ex) => Console.WriteLine($"{log} - {ex.Message}");

        public void SetVerboseMode(bool verbose)
        {
            if (tcpListener is not null)
                throw new InvalidOperationException("Verbose mode can not be changed if the TCP listener has an instance.");

            verboseMode = verbose;
        }
    }
}
