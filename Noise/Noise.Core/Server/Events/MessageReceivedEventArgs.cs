namespace Noise.Core.Server.Events
{
    public class MessageReceivedEventArgs : BufferQueueEventArgs
    {
        public MessageReceivedEventArgs(byte[] packetBufferQueue, string peerEndpoint) : base(packetBufferQueue, peerEndpoint)
        {
        }
    }
}
