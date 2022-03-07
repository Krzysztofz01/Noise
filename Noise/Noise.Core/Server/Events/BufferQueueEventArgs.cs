using System;

namespace Noise.Core.Server.Events
{
    public abstract class BufferQueueEventArgs : EventArgs
    {
        public BufferQueueEventArgs(byte[] packetBufferQueue, string peerEndpoint)
        {
            PacketBufferQueue = packetBufferQueue;
            PeerEndpoint = peerEndpoint;
        }

        public byte[] PacketBufferQueue { get; set; }
        public string PeerEndpoint { get; set; }
    }
}
