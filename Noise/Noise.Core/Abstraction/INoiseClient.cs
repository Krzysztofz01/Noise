using System;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable
    {
        void SendMessage(string receiverPublicKey, string message);
        void SendSignature(string receiverPublicKey);
        void SendDiscovery();
        void SendPing();
    }
}
