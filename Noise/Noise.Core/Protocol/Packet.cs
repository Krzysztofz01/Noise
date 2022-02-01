using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using System;
using System.Linq;

namespace Noise.Core.Protocol
{
    public class Packet
    {
        public static class Factory
        {
            public static Packet<TPayload> FromBuffer<TPayload>(byte[] packetBuffer) where TPayload : Payload<TPayload>, new()
            {
                return Packet<TPayload>.Factory.FromBuffer(packetBuffer);
            }
        }
    }

    public class Packet<TPayload> : IPacket where TPayload : Payload<TPayload>, new()
    {
        private readonly PacketType _packetType;
        private readonly byte[] _checksum;
        private readonly byte[] _payload;
        private readonly Int32 _size;

        private Packet() { }
        private Packet(PacketType packetType, byte[] checksum, byte[] payload, Int32 size)
        {
            _packetType = packetType;
            _checksum = checksum;
            _payload = payload;
            _size = size;
        }

        public TPayload PeekPayload => Payload<TPayload>.Deserialize(_payload);

        public static class Factory
        {
            public static Packet<TPayload> FromPayload(TPayload payload)
            {
                var serializedPayload = payload.Serialize();
                Int32 size = Constants.PacketBaseSize + serializedPayload.Length;

                if (size > Constants.MaximalPacketSize)
                    throw new InvalidOperationException("The payload size is too large.");

                return new Packet<TPayload>(
                    payload.Type,
                    payload.CalculateChecksum(),
                    serializedPayload,
                    size);
            }

            internal static Packet<TPayload> FromBuffer(byte[] packetBuffer)
            {
                if (packetBuffer.Length < Constants.PacketBaseSize)
                    throw new InvalidOperationException("Invalid buffer size. The packet may be corrupted.");

                Int32 size = packetBuffer.ToInt32(0);

                var packetType = (PacketType)packetBuffer.ToInt32(4);

                var checksum = new byte[Constants.ChecksumByteBufferSize];
                Array.Copy(packetBuffer, 8, checksum, 0, Constants.ChecksumByteBufferSize);

                Int32 payloadSize = size - Constants.PacketBaseSize;
                var payload = new byte[payloadSize];
                Array.Copy(packetBuffer, 8 + Constants.ChecksumByteBufferSize, payload, 0, payloadSize);

                var deserializedPayload = Payload<TPayload>.Deserialize(payload);

                deserializedPayload.Validate();

                var checksumCalculated = deserializedPayload.CalculateChecksum();

                if (!checksumCalculated.SequenceEqual(checksum))
                    throw new InvalidOperationException("Checksum not matching. The packet may be corrupted.");

                var deserializedType = deserializedPayload.Type;

                if (packetType != deserializedType)
                    throw new InvalidOperationException("Packet nad payload type not matching. The packet may be corrupted.");

                return new Packet<TPayload>(
                    packetType,
                    checksum,
                    payload,
                    size);
            }
        }

        public byte[] GetBytes()
        {
            var buffer = new byte[_size];

            _size.ToLowEndianByteBuffer().CopyTo(buffer, 0);

            ((Int32)_packetType).ToLowEndianByteBuffer().CopyTo(buffer, 4);
            
            _checksum.CopyTo(buffer, 8);

            _payload.CopyTo(buffer, 8 + Constants.ChecksumByteBufferSize);

            return buffer;
        }

        public override bool Equals(object obj)
        {
            var other = (Packet<TPayload>)obj;

            return other is not null &&
                _checksum.SequenceEqual(other._checksum) &&
                _payload.SequenceEqual(other._payload) &&
                other._size == _size &&
                other._packetType == _packetType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_checksum, _payload, _size, _packetType);
        }
    }
}
