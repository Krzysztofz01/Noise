using Noise.Core.Abstraction;
using Noise.Core.Encryption;
using Noise.Core.Models;
using Noise.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Noise.Core.Services
{
    public class PacketService : IPacketService
    {
        public IEnumerable<(IPacket keyPacket, IPacket discoveryPacket)> CreateDiscoveryPackets(IEnumerable<string> endpoints, IEnumerable<string> publicKeys)
        {
            var discoveryData = new DiscoveryDataModel
            {
                Endpoints = endpoints,
                PublicKeys = publicKeys
            };

            var serializedDiscoveryData = JsonSerializer.Serialize(discoveryData);

            List<(IPacket keyPacket, IPacket discoveryPacket)> packetSets = new();
            
            foreach (var receiverPublicKey in publicKeys)
            {
                var discoveryPayload = Payload.Factory.FromParameters(serializedDiscoveryData);

                var seh = new SymmetricEncryptionHandler();

                var (cipher, key) = seh.Encrypt(discoveryPayload.Serialize());

                var discoveryPacket = Packet.Factory.FromParameters(PacketType.DISCOVERY, cipher);

                var discoveryKeyPayload = Payload.Factory.FromParameters(key);

                var encryptedDiscoveryKeyPayload = AsymmetricEncryptionHandler.Encrypt(discoveryKeyPayload.Serialize(), receiverPublicKey);

                var discoveryKeyPacket = Packet.Factory.FromParameters(PacketType.KEY, encryptedDiscoveryKeyPayload);

                packetSets.Add((discoveryKeyPacket, discoveryPacket));
            }

            return packetSets;
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

        public IPacket CreatePingPacket()
        {
            var payload = Payload.Factory.Empty;

            return Packet.Factory.FromParameters(PacketType.PING, payload.Serialize());
        }

        public (IEnumerable<string> publicKeys, IEnumerable<string> endpoints) RetriveDiscoveryPacket(IPacket keyPacket, IPacket discoveryPacket, string privateKeyXml)
        {
            var discoveryPacketParsed = (Packet)discoveryPacket;
            if (discoveryPacketParsed.Type != PacketType.DISCOVERY)
                throw new ArgumentException("The message packet is incorrect type.", nameof(discoveryPacket));

            var keyPacketParsed = (Packet)keyPacket;
            if (keyPacketParsed.Type != PacketType.KEY)
                throw new ArgumentException("The message packet is incorrect type.", nameof(keyPacket));

            var aeh = new AsymmetricEncryptionHandler(privateKeyXml);

            var serializedDiscoveryKey = aeh.Decrypt(keyPacketParsed.Payload);
            if (serializedDiscoveryKey is null) return (null, null);

            var discoveryKey = Payload.Factory.Deserialize(serializedDiscoveryKey, false).Content;

            var seh = new SymmetricEncryptionHandler();

            var serializedDiscovery = seh.Decrypt(discoveryPacketParsed.Payload, discoveryKey);
            if (serializedDiscovery is null) return (null, null);

            var discovery = Payload.Factory.Deserialize(serializedDiscovery, false);

            var discoveryData = JsonSerializer.Deserialize<DiscoveryDataModel>(discovery.Content);

            return (discoveryData.PublicKeys, discoveryData.Endpoints);
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
