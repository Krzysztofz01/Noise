﻿using Noise.Core.Protocol;
using Xunit;

namespace Noise.Core.Test
{
    public class ProtocolTests
    {
        [Fact]
        public void PacketShouldCreateFromParameters()
        {
            var type = PacketType.MESSAGE;
            var payload = "Hello world!";

            var packet = Packet.Factory.FromParameters(type, payload);

            int expectedSize = 4 + payload.Length + 1;

            Assert.NotNull(packet);

            Assert.Equal(type, packet.Type);
            Assert.Equal(payload, packet.Payload);

            Assert.Equal(expectedSize, packet.Size);
        }

        [Fact]
        public void PacketShouldConvertToBytesBuffer()
        {
            var type = PacketType.MESSAGE;
            var payload = "Hello world!";

            var packet = Packet.Factory.FromParameters(type, payload);

            int expectedPacketBytesLength = 4 + 4 + payload.Length + 1;

            var packetBytes = packet.GetBytes();

            Assert.Equal(expectedPacketBytesLength, packetBytes.Length);
        }

        [Fact]
        public void PacketShouldConvertToBytesBufferAndBackToPacketInstance()
        {
            var type = PacketType.MESSAGE;
            var payload = "Hello world!";

            var packet = Packet.Factory.FromParameters(type, payload);

            var packetBytes = packet.GetBytes();

            var recreatedPacket = Packet.Factory.FromBuffer(packetBytes);

            Assert.Equal(type, recreatedPacket.Type);
            Assert.Equal(payload, recreatedPacket.Payload);
            Assert.Equal(packet.Size, recreatedPacket.Size);
        }
    }
}
