using Noise.Core.Abstraction;
using Noise.Core.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Noise.Core.Client
{
    public class NoiseClient : INoiseClient
    {
        private TcpClient tcpClient;

        public bool Connected => tcpClient.Client.Connected;

        public NoiseClient() => tcpClient = new TcpClient();

        public async Task<bool> ConnectAsync(string peerIpAddress)
        {
            var parsedIpAddress = IPAddress.Parse(peerIpAddress);

            await tcpClient.ConnectAsync(parsedIpAddress, Constants.ProtocolPort);

            return Connected;
        }

        public void Disconnect()
        {
            tcpClient.Close();
            tcpClient = new TcpClient();
        }

        public void Dispose()
        {
            if (tcpClient is not null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
                tcpClient = null;
            }

            GC.SuppressFinalize(this);
        }

        public async Task SendPacketAsync(IPacket packet)
        {
            var networkStream = tcpClient.GetStream();
            var buffer = packet.GetBytes();

            await networkStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
