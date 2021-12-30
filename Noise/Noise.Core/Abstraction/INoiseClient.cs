using Noise.Core.Protocol;
using System;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable
    {
        bool Connected { get; }

        Task SendPacketAsync(IPacket packet);
        Task<Packet> ReceivePacketAsync();
        public Task<bool> ConnectAsync(string peerIpAddress);

        void Disconnect();
    }
}
