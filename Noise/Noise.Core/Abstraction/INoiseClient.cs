using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable
    {
        Task SendMessage(string receiverPublicKey, string message, CancellationToken cancellationToken = default);
        Task SendSignature(string receiverPublicKey, CancellationToken cancellationToken = default);
        Task SendDiscovery(string receiverPublicKey, CancellationToken cancellationToken = default);
        Task SendPing(CancellationToken cancellationToken = default);
    }
}
