using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using System;
using System.Text;

namespace Noise.Core.Protocol
{
    public class Packet : IPacket
    {
        private const Int32 _minimalPossibleSize = 5;

        public PacketType Type { get; private set; }
        public string Payload { get; private set; }
        public Int32 Size => Payload.Length + _minimalPossibleSize;

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[Size + 4];

            Size.ToLowEndianByteBuffer().CopyTo(buffer, 0);
            ((Int32)Type).ToLowEndianByteBuffer().CopyTo(buffer, 4);
            Encoding.ASCII.GetBytes(Payload).CopyTo(buffer, 8);

            return buffer;
        }

        private Packet() { }
        public static class Factory
        {
            public static Packet FromParameters(PacketType type, string payload)
            {
                var packet = new Packet
                {
                    Type = type,
                    Payload = payload ?? string.Empty
                };

                return packet;
            }

            public static Packet FromBuffer(byte[] buffer)
            {
                if (buffer.Length < _minimalPossibleSize)
                    throw new ArgumentException("Invalid buffer size. The packet may be corrupted.", nameof(buffer));

                Int32 length = buffer.ToInt32(0);

                var packet = new Packet
                {
                    Type = (PacketType)buffer.ToInt32(4),
                    Payload = Encoding.ASCII.GetString(buffer, 8, (length - _minimalPossibleSize))
                };

                if (packet.Size != length)
                    throw new ArithmeticException("Invlid packet size after buffer resolving. The packet may be corrupted.");

                return packet;
            }
        }

    }
}
