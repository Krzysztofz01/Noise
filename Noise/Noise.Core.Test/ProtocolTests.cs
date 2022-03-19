using Noise.Core.Exceptions;
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
            string key = Guid.NewGuid().ToString();
            string asymmetricSignature = Guid.NewGuid().ToString();
            string signature = Guid.NewGuid().ToString();

            var payload = SignaturePayload.Factory.Create(signature, key, asymmetricSignature);
            var packet = Packet<SignaturePayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<SignaturePayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            string expectedImc = string.Empty;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
            Assert.Equal(signature, payloadDeserialized.Signature);
            Assert.Equal(key, payloadDeserialized.SenderPublicKey);
            Assert.Equal(asymmetricSignature, payloadDeserialized.SenderAsymmetricSignature);
            Assert.Equal(expectedImc, payloadDeserialized.Certification);
        }

        [Fact]
        public void PacketWithSignaturePayloadShouldCreateSerializeAndDeserializeWithCertification()
        {
            string key = Guid.NewGuid().ToString();
            string asymmetricSignature = Guid.NewGuid().ToString();
            string signature = Guid.NewGuid().ToString();
            string imc = Guid.NewGuid().ToString();

            var payload = SignaturePayload.Factory.Create(signature, key, asymmetricSignature, imc);
            var packet = Packet<SignaturePayload>.Factory.FromPayload(payload);

            Assert.NotNull(packet);

            var packetBuffer = packet.GetBytes();
            var packetDeserialized = Packet.Factory.FromBuffer<SignaturePayload>(packetBuffer);

            Assert.NotNull(packetDeserialized);
            Assert.Equal(packet, packetDeserialized);

            var payloadDeserialized = packetDeserialized.PeekPayload;

            Assert.Equal(payload.Serialize(), payloadDeserialized.Serialize());
            Assert.Equal(signature, payloadDeserialized.Signature);
            Assert.Equal(key, payloadDeserialized.SenderPublicKey);
            Assert.Equal(asymmetricSignature, payloadDeserialized.SenderAsymmetricSignature);
            Assert.Equal(imc, payloadDeserialized.Certification);
        }

        [Fact]
        public void PacketWithSignaurePayloadShouldThrowOnInvalidData()
        {
            string key = null;
            string signature = null;
            string asymmetricSignature = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                SignaturePayload.Factory.Create(signature, key, asymmetricSignature);
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
            var peer1 = MockupPeerConfiguration();

            var peer2 = MockupPeerConfiguration();

            var phs = new PacketHandlingService();

            // Signature exchange: peer1->peer2
            var (keyPacket, signaturePacket, receiverIdentityProve) = phs.CreateSignaturePacket(peer2.PublicKey, peer1.PublicKey, peer1.PrivateKey);

            Assert.NotNull(keyPacket);
            Assert.NotNull(signaturePacket);
            Assert.NotNull(receiverIdentityProve);

            var (signature, senderPublicKey, certification) = phs.ReceiveIdentityProve(keyPacket.GetBytes(), signaturePacket.GetBytes(), peer2.PrivateKey);

            Assert.Equal(receiverIdentityProve, signature);
            Assert.Equal(peer1.PublicKey, senderPublicKey);
            Assert.Equal(string.Empty, certification);
        }

        [Fact]
        public void PacketHandlingServiceShouldThrowOnSignaturePacketSpoofing()
        {
            var peer1 = MockupPeerConfiguration();

            var peer2 = MockupPeerConfiguration();

            var peer3 = MockupPeerConfiguration();

            var phs = new PacketHandlingService();

            // Signature exchange: peer1(as peer2)->peer3
            var (keyPacket, signaturePacket, receiverIdentityProve) = phs.CreateSignaturePacket(peer3.PublicKey, peer2.PublicKey, peer1.PrivateKey);

            Assert.NotNull(keyPacket);
            Assert.NotNull(signaturePacket);
            Assert.NotNull(receiverIdentityProve);

            Assert.Throws<PacketRejectedException>(() =>
            {
                _ = phs.ReceiveIdentityProve(keyPacket.GetBytes(), signaturePacket.GetBytes(), peer3.PrivateKey);
            });
        }

        [Fact]
        public void PacketHandlingServiceShouldHandleMessagePackets()
        {
            var peer1 = MockupPeerConfiguration();
            string peer1SignatureCreatedForPeer2;

            var peer2 = MockupPeerConfiguration();
            string peer2SignautreReceivedFromPeer1;

            var phs = new PacketHandlingService();

            // Signature exchange: peer1->peer2
            var (signatureKeyPacket, signaturePacket, receiversSignature) = phs.CreateSignaturePacket(peer2.PublicKey, peer1.PublicKey, peer1.PrivateKey);
            peer1SignatureCreatedForPeer2 = receiversSignature;
            
            var (signature, senderPublicKey, certification) = phs.ReceiveIdentityProve(signatureKeyPacket.GetBytes(), signaturePacket.GetBytes(), peer2.PrivateKey);
            peer2SignautreReceivedFromPeer1 = signature;

            // Message exchange: peer2->peer1
            var message = "Hello World!";
            var (messageKeyPacket, messagePacket) = phs.CreateMessagePackets(peer2SignautreReceivedFromPeer1, peer1.PublicKey, message);
            var (receivedSignature, receivedMessage) = phs.ReceiveMessage(messageKeyPacket.GetBytes(), messagePacket.GetBytes(), peer1.PrivateKey);

            Assert.Equal(message, receivedMessage);
            Assert.Equal(peer1SignatureCreatedForPeer2, receivedSignature);
            Assert.Equal(peer1.PublicKey, senderPublicKey);
            Assert.Equal(string.Empty, certification);
        }

        [Fact]
        public void PacketHandlingServiceShouldHandleDiscoveryPackets()
        {
            var peer1 = MockupPeerConfiguration();
            string peer1SignatureCreatedForPeer2;

            var peer2 = MockupPeerConfiguration();
            var endpoints = new List<string>() { "Hello World!" };
            var publicKeys = new List<string>() { "Hello World!" };
            string peer2SignautreReceivedFromPeer1;

            var phs = new PacketHandlingService();

            // Signature exchange: peer1->peer2
            var (signatureKeyPacket, signaturePacket, receiversSignature) = phs.CreateSignaturePacket(peer2.PublicKey, peer1.PublicKey, peer1.PrivateKey);
            peer1SignatureCreatedForPeer2 = receiversSignature;

            var (signature, senderPublicKey, certification) = phs.ReceiveIdentityProve(signatureKeyPacket.GetBytes(), signaturePacket.GetBytes(), peer2.PrivateKey);
            peer2SignautreReceivedFromPeer1 = signature;

            // Discovery exchange: peer2->peer1
            var (discoveryKeyPacket, discoveryPacket) = phs.CreateDiscoveryPackets(peer2SignautreReceivedFromPeer1, peer1.PublicKey, endpoints, publicKeys);
            var (receivedEndpoint, receivedPublicKeys, receivedSignature) = phs.ReceiveDiscoveryCollections(discoveryKeyPacket.GetBytes(), discoveryPacket.GetBytes(), peer1.PrivateKey);
            
            Assert.Equal(endpoints, receivedEndpoint);
            Assert.Equal(publicKeys, receivedPublicKeys);
            Assert.Equal(peer1SignatureCreatedForPeer2, receivedSignature);
            Assert.Equal(peer1.PublicKey, senderPublicKey);
            Assert.Equal(string.Empty, certification);
        }

        [Fact]
        public void PacketHandlingServiceShouldHandlePingPackets()
        {
            var phs = new PacketHandlingService();

            var pingPacket = phs.CreatePingPacket();

            Assert.NotNull(pingPacket);
        }

        [Fact]
        public void PacketBufferAndQueueBilderShouldBuild()
        {
            var pingPayload = PingPayload.Factory.Create();
            var pingPacket = Packet<PingPayload>.Factory.FromPayload(pingPayload);

            string publicKeys = Guid.NewGuid().ToString();
            string endpoints = Guid.NewGuid().ToString();
            string signature = Guid.NewGuid().ToString();

            var discoveryPayload = DiscoveryPayload.Factory.Create(publicKeys, endpoints, signature);
            var discoveryPacket = Packet<DiscoveryPayload>.Factory.FromPayload(discoveryPayload);

            var queueBuffer = PacketBufferStreamBuilder
                .Create()
                .InsertPacket(pingPacket)
                .InsertPacket(discoveryPacket)
                .Build();

            var byteQueue = PacketBufferQueueBuilder
                .Create()
                .InsertBuffer(queueBuffer)
                .Build();

            var builtPingPacket = Packet.Factory.FromBuffer<PingPayload>(byteQueue.Dequeue());
            var builtDiscoveryPacket = Packet.Factory.FromBuffer<DiscoveryPayload>(byteQueue.Dequeue());

            Assert.Equal(pingPacket, builtPingPacket);
            Assert.Equal(discoveryPacket, builtDiscoveryPacket);
        }

        public PeerConfiguration MockupPeerConfiguration()
        {
            return PeerConfiguration.Factory.Initialize("Hello World!");
        }
    }
}
