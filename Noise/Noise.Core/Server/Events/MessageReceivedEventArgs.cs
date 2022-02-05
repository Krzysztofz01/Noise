namespace Noise.Core.Server.Events
{
    public class MessageReceivedEventArgs : BufferQueueEventArgs
    {
        public MessageReceivedEventArgs(byte[] packetBufferQueue) : base(packetBufferQueue)
        {
        }
    }
}
