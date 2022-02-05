using Noise.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Noise.Core.Protocol
{
    public class PacketBufferQueueBuilder
    {
        private const int _baseSizeOffset = 4;
        private readonly List<byte> _buffer;
    
        public PacketBufferQueueBuilder InsertBuffer(byte[] buffer)
        {
            _buffer.AddRange(buffer);
            return this;
        }

        public Queue<byte[]> Build()
        {
            var buffer = _buffer.ToArray();

            if (buffer.Length < _baseSizeOffset)
                throw new InvalidOperationException("The buffer length is not valid. No data");

            var packetQueue = new Queue<byte[]>();
            int expectedSize = buffer.ToInt32(0) + _baseSizeOffset;

            if (_buffer.Count != expectedSize)
                throw new InvalidOperationException("The buffer length is not valid. The stream may be corrupted.");

            int bytesCounted = _baseSizeOffset;
            buffer = buffer[_baseSizeOffset..];

            while (buffer.Length > 0)
            {
                int packetLength = buffer.ToInt32(0);

                var currentPacket = buffer[0..packetLength];

                bytesCounted += currentPacket.Length;

                packetQueue.Enqueue(currentPacket);

                buffer = buffer[packetLength..];
            }

            if (_buffer.Count != bytesCounted)
                throw new InvalidOperationException("Buffer queue building error. Invalid buffer offset.");

            return packetQueue;
        }

        public static PacketBufferQueueBuilder Create() => new();
        private PacketBufferQueueBuilder() => _buffer = new List<byte>();
    }
}
