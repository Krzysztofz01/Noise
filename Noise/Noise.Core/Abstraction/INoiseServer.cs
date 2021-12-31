using Noise.Core.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseServer : IDisposable
    {
        event EventHandler<ClientDisconnectedEventsArgs> OnClientDisconnected;
        event EventHandler<PacketReceivedEventsArgs> OnMessageReceived;
        event EventHandler<PacketReceivedEventsArgs> OnPingReceived;
        event EventHandler<PacketReceivedEventsArgs> OnKeyReceived;
        event EventHandler<PacketReceivedEventsArgs> OnDiscoveryReceived;

        Task StartAsync(CancellationToken cancellationToken);
    }
}
