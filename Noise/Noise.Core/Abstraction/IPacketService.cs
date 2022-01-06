using System.Collections.Generic;

namespace Noise.Core.Abstraction
{
    public interface IPacketService
    {
        (IPacket keyPacket, IPacket messagePacket) CreateMessagePackets(string senderPublicKey, string receiverPublicKey, string message);
        IEnumerable<(IPacket keyPacket, IPacket discoveryPacket)> CreateDiscoveryPackets(IEnumerable<string> endpoints, IEnumerable<string> publicKeys);
        IPacket CreatePingPacket();

        (string publicKey, string message) RetriveMessagePacket(IPacket keyPacket, IPacket messagePacket, string privateKeyXml);
        (IEnumerable<string> publicKeys, IEnumerable<string> endpoints) RetriveDiscoveryPacket(IPacket keyPacket, IPacket discoveryPacket, string privateKeyXml);
    }
}
