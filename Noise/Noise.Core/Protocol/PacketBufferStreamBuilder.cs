using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using System.Collections.Generic;

namespace Noise.Core.Protocol
{
    public class PacketBufferStreamBuilder
    {
        private readonly Queue<IPacket> _packets;

        public PacketBufferStreamBuilder InsertPacket(IPacket packet)
        {
            _packets.Enqueue(packet);
            return this;
        }

        public byte[] Build()
        {
            var buffer = new List<byte>();
            int size = 0;
            
            foreach (var packet in _packets)
            {
                var packetBuffer = packet.GetBytes();
                size += packetBuffer.Length;

                buffer.AddRange(packetBuffer);
            }

            var sizeBuffer = size.ToLowEndianByteBuffer();
            buffer.InsertRange(0, sizeBuffer);

            return buffer.ToArray();
        }

        public static PacketBufferStreamBuilder Create() => new(); 

        private PacketBufferStreamBuilder() => _packets = new Queue<IPacket>();
    }
}
