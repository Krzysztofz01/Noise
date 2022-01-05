using Noise.Core.Abstraction;
using Noise.Core.Encryption;
using Noise.Core.Protocol;
using System;
using System.Collections.Generic;

namespace Noise.Core.Services
{
    public class PacketService : IPacketService
    {
        public IEnumerable<IPacket> CreateDiscoveryPackets(IEnumerable<string> endpoints, IEnumerable<string> publicKeys)
        {
            throw new NotImplementedException();
        }

        public (IPacket keyPacket, IPacket messagePacket) CreateMessagePackets(string senderPublicKey, string receiverPublicKey, string message)
        {
            var messagePayload = Payload.Factory.FromParameters(senderPublicKey, message, true);

            var seh = new SymmetricEncryptionHandler();

            var (cipher, key) = seh.Encrypt(messagePayload.Serialize());

            var messagePacket = Packet.Factory.FromParameters(PacketType.MESSAGE, cipher);

            var messageKeyPayload = Payload.Factory.FromParameters(key);

            var encryptedMessageKeyPayload = AsymmetricEncryptionHandler.Encrypt(messageKeyPayload.Serialize(), receiverPublicKey);

            var messageKeyPacket = Packet.Factory.FromParameters(PacketType.KEY, encryptedMessageKeyPayload);

            return (messageKeyPacket, messagePacket);
        }

        public IPacket CreatePingPacket(IEnumerable<string> endpoints)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(string publicKey, string endpoint)> RetriveDiscoveryPacket(IPacket discoveryPacket)
        {
            throw new NotImplementedException();
        }

        public (string publicKey, string message) RetriveMessagePacket(IPacket keyPacket, IPacket messagePacket, string privateKeyXml)
        {
            var messagePacketParsed = (Packet)messagePacket;
            if (messagePacketParsed.Type != PacketType.MESSAGE)
                throw new ArgumentException("The message packet is incorrect type.", nameof(messagePacket));

            var keyPacketParsed = (Packet)keyPacket;
            if (keyPacketParsed.Type != PacketType.KEY)
                throw new ArgumentException("The message packet is incorrect type.", nameof(keyPacket));

            var aeh = new AsymmetricEncryptionHandler(privateKeyXml);

            var serializedMessageKey = aeh.Decrypt(keyPacketParsed.Payload);
            if (serializedMessageKey is null) return (null, null);

            var messageKey = Payload.Factory.Deserialize(serializedMessageKey, false).Content;

            var seh = new SymmetricEncryptionHandler();

            var serializedMessage = seh.Decrypt(messagePacketParsed.Payload, messageKey);
            if (serializedMessage is null) return (null, null);

            var message = Payload.Factory.Deserialize(serializedMessage);

            return (message.PublicKey, message.Content);
        }
    }
}
