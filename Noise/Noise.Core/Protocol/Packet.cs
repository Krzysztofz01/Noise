using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using System;
using System.Text;

namespace Noise.Core.Protocol
{
    public class Packet : IPacket
    {
        public PacketType Type { get; private set; }
        public string Payload { get; private set; }
        public Int32 Size => Payload.Length + Constants.MinimalPacketBytesSize;

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[Size + 4];

            Size.ToLowEndianByteBuffer().CopyTo(buffer, 0);
            ((Int32)Type).ToLowEndianByteBuffer().CopyTo(buffer, 4);
            Encoding.ASCII.GetBytes(Payload).CopyTo(buffer, 8);

            return buffer;
        }

        public Payload PayloadDeserialized =>
            Protocol.Payload.Factory.Deserialize(Payload, Type == PacketType.MESSAGE);

        private static void ValidatePacketSize(Int32 size)
        {
            if (size + 4 > Constants.MaximalPacketBytesSize)
                throw new ArgumentException("The given arguments created a packet that is outside the size limits.");
        }

        private Packet() { }
        public static class Factory
        {
            public static Packet FromParameters(PacketType type, string encryptedSerializedPayload)
            {
                var packet = new Packet
                {
                    Type = type,
                    Payload = encryptedSerializedPayload
                };

                ValidatePacketSize(packet.Size);

                return packet;
            }

            public static Packet FromBuffer(byte[] buffer)
            {
                if (buffer.Length < Constants.MinimalPacketBytesSize)
                    throw new ArgumentException("Invalid buffer size. The packet may be corrupted.", nameof(buffer));

                Int32 length = buffer.ToInt32(0);

                var packet = new Packet
                {
                    Type = (PacketType)buffer.ToInt32(4),
                    Payload = Encoding.ASCII.GetString(buffer, 8, (length - Constants.MinimalPacketBytesSize))
                };

                if (packet.Size != length)
                    throw new ArithmeticException("Invlid packet size after buffer resolving. The packet may be corrupted.");

                ValidatePacketSize(packet.Size);

                return packet;
            }
        }
    }
}
