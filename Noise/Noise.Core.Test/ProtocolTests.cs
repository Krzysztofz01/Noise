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
            Assert.Equal(signature, payloadDeserialized.IdentityProve);
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
            var publicKeys = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            var endpoints = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            var payload = DiscoveryPayload.Factory.Create(publicKeys, endpoints);
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
        }

        [Fact]
        public void PacketWithKeyDiscoveryShouldThrowOnInvalidData()
        {
            List<string> publicKeys = null;
            List<string> endpoints = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                DiscoveryPayload.Factory.Create(publicKeys, endpoints);
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
    }
}
