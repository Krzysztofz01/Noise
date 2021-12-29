using Noise.Core.Protocol;
using System;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable, IAsyncDisposable
    {
        bool Connected { get; }

        Task SendPacketAsync(IPacket packet);
        Task<Packet> ReceivePacketAsync();
        Task<bool> ConnectAsync(string peer);

        void Disconnect();
    }
}
