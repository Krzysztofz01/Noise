using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.Collections.Generic;
using Xunit;

namespace Noise.Core.Test
{
    public class ProtocolTests
    {
        [Fact]
        public void PayloadSerializerShouldSerializeAndDeserializePayloadDictionary()
        {
            var payload = new Dictionary<string, string>()
            {
                { "Hello", "World" },
                { "Foo", "Bar"}
            };

            var serializedPayload = PayloadSerializer.Serialize(payload);

            var deserializedPayload = PayloadSerializer.Deserialize(serializedPayload);

            Assert.Equal(payload, deserializedPayload);
            Assert.NotNull(deserializedPayload);
        }

        [Fact]
        public void PacketWithPingPayloadShouldCreateSerializeAndDeserialize()
        {
            var payload = PingPayload.Factory.Create();
            var packet = Packet<PingPayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<PingPayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
        }

        [Fact]
        public void PacketWithSignaturePayloadShouldCreateSerializeAndDeserialize()
        {
            string signature = Guid.NewGuid().ToString();

            var payload = SignaturePayload.Factory.Create(signature);
            var packet = Packet<SignaturePayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<SignaturePayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
            Assert.Equal(signature, payloadDeserialized.Signature);
        }

        [Fact]
        public void PacketWithSignaurePayloadShouldThrowOnInvalidData()
        {
            string signature = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                SignaturePayload.Factory.Create(signature);
            });
        }

        [Fact]
        public void PacketWithMessagePayloadShouldCreateSerializeAndDeserialize()
        {
            string cipher = Guid.NewGuid().ToString();
            string signature = Guid.NewGuid().ToString();

            var payload = MessagePayload.Factory.Create(cipher, signature);
            var packet = Packet<MessagePayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<MessagePayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
            Assert.Equal(cipher, payloadDeserialized.MessageCipher);
            Assert.Equal(signature, payloadDeserialized.IdentityProve);
        }

        [Fact]
        public void PacketWithMessagePayloadShouldThrowOnInvalidData()
        {
            string cipher = null;
            string signature = null;
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                MessagePayload.Factory.Create(cipher, signature);
            });
        }

        [Fact]
        public void PacketWithKeyPayloadShouldCreateSerializeAndDeserialize()
        {
            string key = Guid.NewGuid().ToString();
            string signature = Guid.NewGuid().ToString();

            var payload = KeyPayload.Factory.Create(key, signature);
            var packet = Packet<KeyPayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<KeyPayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
            Assert.Equal(key, payloadDeserialized.MessageKey);
            Assert.Equal(signature, payloadDeserialized.IdentityProveKey);
        }

        [Fact]
        public void PacketWithKeyPayloadShouldThrowOnInvalidData()
        {
            string key = null;
            string signature = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                KeyPayload.Factory.Create(key, signature);
            });
        }

        [Fact]
        public void PacketWithDiscoveryPayloadShouldCreateSerializeAndDeserialize()
        {
            string publicKeys = Guid.NewGuid().ToString();
            string endpoints = Guid.NewGuid().ToString();
            string signature = Guid.NewGuid().ToString();

            var payload = DiscoveryPayload.Factory.Create(publicKeys, endpoints, signature);
            var packet = Packet<DiscoveryPayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<DiscoveryPayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
            Assert.Equal(publicKeys, payloadDeserialized.PublicKeys);
            Assert.Equal(endpoints, payloadDeserialized.Endpoints);
            Assert.Equal(signature, payloadDeserialized.IdentityProve);
        }

        [Fact]
        public void PacketWithKeyDiscoveryShouldThrowOnInvalidData()
        {
            string publicKeys = null;
            string endpoints = null;
            string signature = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                DiscoveryPayload.Factory.Create(publicKeys, endpoints, signature);
            });
        }

        [Fact]
        public void PacketShouldThrowWhenDeserializingFromBufferDifferentTypeThanSpecified()
        {
            var pingPayload = PingPayload.Factory.Create();
            var pingPacket = Packet<PingPayload>.Factory.FromPayload(pingPayload);
            var packetBuffer = pingPacket.GetBytes();

            Assert.Throws<InvalidOperationException>(() =>
            {
                Packet.Factory.FromBuffer<MessagePayload>(packetBuffer);
            });
        }

        [Fact]
        public void PacketHandlingServiceShouldHandleSignaturePackets()
        {
            var receiver = PeerConfiguration.Factory.Initialize();

            var phs = new PacketHandlingService();

            var (signaturePacket, receiversSignature) = phs.CreateSignaturePacket(receiver.PublicKey);

            Assert.NotNull(signaturePacket);
            Assert.NotNull(receiversSignature);

            var receivedSignature = phs.ReceiveIdentityProve(signaturePacket.GetBytes(), receiver.PrivateKeyXml);

            Assert.NotNull(receivedSignature);
            Assert.Equal(receiversSignature, receivedSignature);
        }

        [Fact]
        public void PacketHandlingServiceShouldHandleMessagePackets()
        {
            var peer1 = PeerConfiguration.Factory.Initialize();
            string peer1SignatureCreatedForPeer2;

            var peer2 = PeerConfiguration.Factory.Initialize();
            string peer2SignautreReceivedFromPeer1;

            var phs = new PacketHandlingService();

            // Signature exchange: peer1->peer2
            var (signaturePacket, receiversSignature) = phs.CreateSignaturePacket(peer2.PublicKey);
            peer1SignatureCreatedForPeer2 = receiversSignature;
            peer2SignautreReceivedFromPeer1 = phs.ReceiveIdentityProve(signaturePacket.GetBytes(), peer2.PrivateKeyXml);

            // Message exchange: peer2->peer1
            var message = "Hello World!";
            var (keyPacket, messagePacket) = phs.CreateMessagePackets(peer2SignautreReceivedFromPeer1, peer1.PublicKey, message);
            var (receivedSignature, receivedMessage) = phs.ReceiveMessage(keyPacket.GetBytes(), messagePacket.GetBytes(), peer1.PrivateKeyXml);

            Assert.Equal(message, receivedMessage);
            Assert.Equal(peer1SignatureCreatedForPeer2, receivedSignature);
        }

        [Fact]
        public void PacketHandlingServiceShouldHandleDiscoveryPackets()
        {
            var peer1 = PeerConfiguration.Factory.Initialize();
            string peer1SignatureCreatedForPeer2;

            var peer2 = PeerConfiguration.Factory.Initialize();
            var endpoints = new List<string>() { "Hello World!" };
            var publicKeys = new List<string>() { "Hello World!" };
            string peer2SignautreReceivedFromPeer1;

            var phs = new PacketHandlingService();

            // Signature exchange: peer1->peer2
            var (signaturePacket, receiversSignature) = phs.CreateSignaturePacket(peer2.PublicKey);
            peer1SignatureCreatedForPeer2 = receiversSignature;
            peer2SignautreReceivedFromPeer1 = phs.ReceiveIdentityProve(signaturePacket.GetBytes(), peer2.PrivateKeyXml);

            // Discovery exchange: peer2->peer1
            var (keyPacket, discoveryPacket) = phs.CreateDiscoveryPackets(peer2SignautreReceivedFromPeer1, peer1.PublicKey, endpoints, publicKeys);
            var (receivedEndpoint, receivedPublicKeys, receivedSignature) = phs.ReceiveDiscoveryCollections(keyPacket.GetBytes(), discoveryPacket.GetBytes(), peer1.PrivateKeyXml);
            
            Assert.Equal(endpoints, receivedEndpoint);
            Assert.Equal(publicKeys, receivedPublicKeys);
            Assert.Equal(peer1SignatureCreatedForPeer2, receivedSignature);
        }

        [Fact]
        public void PacketHandlingServiceShouldHandlePingPackets()
        {
            var phs = new PacketHandlingService();

            var pingPacket = phs.CreatePingPacket();

            Assert.NotNull(pingPacket);
        }
    }
}
