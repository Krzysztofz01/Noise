using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Noise.Core.Client
{
    public class NoiseClient : INoiseClient
    {
        private readonly PeerConfiguration _peerConfiguration;

        private TcpClient tcpClient;

        public bool Connected => tcpClient.Client.Connected;

        public NoiseClient(PeerConfiguration peerConfiguration)
        {
             tcpClient = new TcpClient();

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));
        }

        private async Task<bool> ConnectAsync(string peerIpAddress)
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
            foreach (var endpoint in _peerConfiguration.GetEndpoints())
            {
                await ConnectAsync(endpoint);

                var networkStream = tcpClient.GetStream();
                var buffer = packet.GetBytes();

                await networkStream.WriteAsync(buffer, 0, buffer.Length);

                Disconnect();
            }
        }

        public async Task SendPacketsAsync(IEnumerable<IPacket> packets)
        {
            foreach (var endpoint in _peerConfiguration.GetEndpoints())
            {
                await ConnectAsync(endpoint);
                
                foreach (var packet in packets)
                {
                    var networkStream = tcpClient.GetStream();
                    var buffer = packet.GetBytes();

                    await networkStream.WriteAsync(buffer, 0, buffer.Length);
                }

                Disconnect();
            }
        }
    }
}
