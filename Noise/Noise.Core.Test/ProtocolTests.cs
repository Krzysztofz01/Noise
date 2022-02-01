﻿using Noise.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
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
            var packetDeserialized = Packet<PingPayload>.Factory.FromBuffer(packetBuffer);

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
            var packetDeserialized = Packet<SignaturePayload>.Factory.FromBuffer(packetBuffer);

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

        /*[Fact]
        public void PacketPayloadShouldCreateFromParametersWithPublicKey()
        {
            string publicKey = MockupPublicKey();
            string content = "Hello World!";

            var payload = Payload.Factory.FromParameters(publicKey, content);

            Assert.Equal(publicKey, payload.PublicKey);
            Assert.Equal(content, payload.Content);
        }

        [Fact]
        public void PacketPayloadShouldCreateFromParametersWithoutPublicKey()
        {
            string content = "Hello World!";

            var payload = Payload.Factory.FromParameters(content);

            Assert.Equal(content, payload.Content);
        }

        [Fact]
        public void PacketPayloadWithoutPublicKeyShouldThrowWhenKeyAccesed()
        {
            string content = "Hello World!";

            var payloadWithoutValidation = Payload.Factory.FromParameters(null, content, false);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var publicKey = payloadWithoutValidation.PublicKey;
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Payload.Factory.FromParameters(null, content, true);
            });
        }

        [Fact]
        public void PacketPayloadShouldSerializeAndDeserialize()
        {
            string publicKey = MockupPublicKey();
            string content = "Hello World!";

            var payload = Payload.Factory.FromParameters(publicKey, content);

            string serializedPayload = payload.Serialize();

            var deserializedPayload = Payload.Factory.Deserialize(serializedPayload);

            Assert.Equal(publicKey, deserializedPayload.PublicKey);
            Assert.Equal(content, deserializedPayload.Content);
        }

        [Fact]
        public void PacketPayloadShouldNotValidateWithoutPublicKeyAndValidationOn()
        {
            string publicKey = string.Empty;
            string content = "Hello World!";

            Assert.Throws<ArgumentException>(() =>
            {
                Payload.Factory.FromParameters(publicKey, content);
            });
        }

        [Fact]
        public void PacketShouldCreateFromParameters()
        {
            var type = PacketType.PING;

            var payloadKey = MockupPublicKey();
            var payloadContent = "Hello world!";

            var payload = Payload.Factory.FromParameters(payloadKey, payloadContent);
            var packet = Packet.Factory.FromParameters(type, payload.Serialize());

            int expectedSize = Constants.MinimalPacketBytesSize + Constants.PublicKeyStringSize + Constants.MinimalPayloadStringSize + payloadContent.Length;

            var recreatedPayload = Payload.Factory.Deserialize(packet.Payload);

            Assert.NotNull(packet);

            Assert.Equal(type, packet.Type);
            Assert.Equal(payloadContent, recreatedPayload.Content);

            Assert.Equal(expectedSize, packet.Size);
        }

        [Fact]
        public void PacketShouldConvertToBytesBuffer()
        {
            var type = PacketType.PING;

            var payloadKey = MockupPublicKey();
            var payloadContent = "Hello world!";

            var payload = Payload.Factory.FromParameters(payloadKey, payloadContent);

            var packet = Packet.Factory.FromParameters(type, payload.Serialize());

            int expectedPacketBytesLength = 4 + 4 + payload.Content.Length + Constants.MinimalPayloadStringSize + Constants.PublicKeyStringSize + 1;

            var packetBytes = packet.GetBytes();

            Assert.Equal(expectedPacketBytesLength, packetBytes.Length);
        }

        [Fact]
        public void PacketShouldConvertToBytesBufferAndBackToPacketInstance()
        {
            var type = PacketType.PING;
            var payloadContent = "Hello world!";

            var payload = Payload.Factory.FromParameters(MockupPublicKey(), payloadContent);
            var packet = Packet.Factory.FromParameters(type, payload.Serialize());

            var packetBytes = packet.GetBytes();

            var recreatedPacket = Packet.Factory.FromBuffer(packetBytes);
            var recreatedPayload = Payload.Factory.Deserialize(recreatedPacket.Payload);

            Assert.Equal(type, recreatedPacket.Type);
            Assert.Equal(payloadContent, recreatedPayload.Content);
            Assert.Equal(packet.Size, recreatedPacket.Size);
        }

        [Fact]
        public void PacketOutOfSizeLimitsShouldThrowAnException()
        {
            var type = PacketType.PING;

            var payloadContentStringBuilder = new StringBuilder(string.Empty);
            for (int i=0; i < Constants.MaximalPacketBytesSize + 1; i++)
            {
                payloadContentStringBuilder.Append('A');
            }

            var payload = Payload.Factory.FromParameters(MockupPublicKey(), payloadContentStringBuilder.ToString());

            Assert.Throws<ArgumentException>(() =>
            {
                Packet.Factory.FromParameters(type, payload.Serialize());
            });
        }

        private string MockupPublicKey()
        {
            StringBuilder sb = new(string.Empty);
            for (int i = 0; i < Constants.PublicKeyStringSize; i++) sb.Append('A');
            return sb.ToString();
        }*/
    }
}
