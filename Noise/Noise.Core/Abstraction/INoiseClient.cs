using System;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable
    {
        bool Connected { get; }

        Task SendPacketAsync(IPacket packet);
        public Task<bool> ConnectAsync(string peerIpAddress);

        void Disconnect();
    }
}
