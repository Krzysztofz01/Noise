using System.Collections.Generic;

namespace Noise.Core.Abstraction
{
    public interface IPacketHandlingService
    {
        (IPacket keyPacket, IPacket messagePacket) CreateMessagePackets(string senderIdentityProve, string receiverPublicKey, string message);
        (string senderIdentityProve, string message) ReceiveMessage(byte[] keyPacketBuffer, byte[] messagePacketBuffer, string receiverPrivateKey);

        public (IPacket keyPacket, IPacket signaturePacket, string receiverIdentityProve) CreateSignaturePacket(string receiverPublicKey, string senderPublicKey, string senderPrivateKey, string certification = null);
        public (string signature, string senderPublicKey, string certification) ReceiveIdentityProve(byte[] keyPacketBuffer, byte[] signaturePacketBuffer, string receiverPrivateKey);

        (IPacket keyPacket, IPacket discoveryPacket) CreateDiscoveryPackets(string senderIdentityProve, string receiverPublicKey, IEnumerable<string> endpoints, IEnumerable<string> publicKeys);
        (IEnumerable<string> endpoints, IEnumerable<string> publicKeys, string senderIdentityProve) ReceiveDiscoveryCollections(byte[] keyPacketBuffer, byte[] discoveryPacketBuffer, string receiverPrivateKey);

        IPacket CreatePingPacket();
    }
}
