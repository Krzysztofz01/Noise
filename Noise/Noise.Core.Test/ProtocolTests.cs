using Noise.Core.Protocol;
using System;
using System.Text;
using Xunit;

namespace Noise.Core.Test
{
    public class ProtocolTests
    {
        [Fact]
        public void PacketPayloadShouldCreateFromParameters()
        {
            string publicKey = MockupPublicKey();
            string content = "Hello World!";

            var payload = Payload.Factory.FromParameters(publicKey, content);

            Assert.Equal(publicKey, payload.PublicKey);
            Assert.Equal(content, payload.Content);
        }

        [Fact]
        public void PacketPayloadShouldSerializeAndDeserialize()
        {
            string publicKey = MockupPublicKey();
            string content = "Hello World!";

            var payload = Payload.Factory.FromParameters(publicKey, content);

            string serializedPayload = payload.ToString();

            var deserializedPayload = Payload.Factory.FromString(serializedPayload);

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
            var type = PacketType.MESSAGE;
            var payloadContent = "Hello world!";

            var payload = Payload.Factory.FromParameters(MockupPublicKey(), payloadContent);
            var packet = Packet.Factory.FromParameters(type, payload);

            int expectedSize = 4 + payloadContent.Length + Constants.PublicKeySize + 1;

            Assert.NotNull(packet);

            Assert.Equal(type, packet.Type);
            Assert.Equal(payloadContent, packet.PayloadDeserialized.Content);

            Assert.Equal(expectedSize, packet.Size);
        }

        [Fact]
        public void PacketShouldConvertToBytesBuffer()
        {
            var type = PacketType.MESSAGE;
            var payloadContent = "Hello world!";

            var payload = Payload.Factory.FromParameters(MockupPublicKey(), payloadContent);
            var packet = Packet.Factory.FromParameters(type, payload);

            int expectedPacketBytesLength = 4 + 4 + payloadContent.Length + Constants.PublicKeySize + 1;

            var packetBytes = packet.GetBytes();

            Assert.Equal(expectedPacketBytesLength, packetBytes.Length);
        }

        [Fact]
        public void PacketShouldConvertToBytesBufferAndBackToPacketInstance()
        {
            var type = PacketType.MESSAGE;
            var payloadContent = "Hello world!";

            var payload = Payload.Factory.FromParameters(MockupPublicKey(), payloadContent);
            var packet = Packet.Factory.FromParameters(type, payload);

            var packetBytes = packet.GetBytes();

            var recreatedPacket = Packet.Factory.FromBuffer(packetBytes);

            Assert.Equal(type, recreatedPacket.Type);
            Assert.Equal(payloadContent, recreatedPacket.PayloadDeserialized.Content);
            Assert.Equal(packet.Size, recreatedPacket.Size);
        }

        [Fact]
        public void PacketOutOfSizeLimitsShouldThrowAnException()
        {
            var type = PacketType.MESSAGE;

            var payloadContentStringBuilder = new StringBuilder(string.Empty);
            for (int i=0; i < Constants.MaximalPacketBytesSize + 1; i++)
            {
                payloadContentStringBuilder.Append('A');
            }

            var payload = Payload.Factory.FromParameters(MockupPublicKey(), payloadContentStringBuilder.ToString());

            Assert.Throws<ArgumentException>(() =>
            {
                Packet.Factory.FromParameters(type, payload);
            });
        }

        private string MockupPublicKey()
        {
            StringBuilder sb = new(string.Empty);
            for (int i = 0; i < Constants.PublicKeySize; i++) sb.Append('A');
            return sb.ToString();
        }
    }
}
