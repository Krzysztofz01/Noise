namespace Noise.Core.Server.Events
{
    public class DiscoveryReceivedEventArgs : BufferQueueEventArgs
    {
        public DiscoveryReceivedEventArgs(byte[] packetBufferQueue, string peerEndpoint) : base(packetBufferQueue, peerEndpoint)
        {
        }
    }
}
