using System;

namespace Noise.Core.Server.Events
{
    public abstract class BufferQueueEventArgs : EventArgs
    {
        public BufferQueueEventArgs(byte[] packetBufferQueue) =>
            PacketBufferQueue = packetBufferQueue;

        public byte[] PacketBufferQueue { get; set; }
    }
}
