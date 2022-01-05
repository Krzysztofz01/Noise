using System.Collections.Generic;

namespace Noise.Core.Abstraction
{
    public interface IPacketService
    {
        (IPacket keyPacket, IPacket messagePacket) CreateMessagePackets(string senderPublicKey, string receiverPublicKey, string message);
        IEnumerable<IPacket> CreateDiscoveryPackets(IEnumerable<string> endpoints, IEnumerable<string> publicKeys);
        IPacket CreatePingPacket(IEnumerable<string> endpoints);

        (string publicKey, string message) RetriveMessagePacket(IPacket keyPacket, IPacket messagePacket, string privateKeyXml);
        IEnumerable<(string publicKey, string endpoint)> RetriveDiscoveryPacket(IPacket discoveryPacket);
    }
}
