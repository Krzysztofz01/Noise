namespace Noise.Core.Server.Events
{
    public class BroadcastReceivedEventArgs : BufferQueueEventArgs
    {
        public BroadcastReceivedEventArgs(byte[] packetBufferQueue, string peerEndpoint) : base(packetBufferQueue, peerEndpoint)
        {
        }
    }
}
