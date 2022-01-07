using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable
    {
        bool Connected { get; }

        Task SendPacketAsync(IPacket packet);
        Task SendPacketsAsync(IEnumerable<IPacket> packets);

        void Disconnect();
    }
}
