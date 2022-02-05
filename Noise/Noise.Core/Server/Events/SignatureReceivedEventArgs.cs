namespace Noise.Core.Server.Events
{
    public class SignatureReceivedEventArgs : BufferQueueEventArgs
    {
        public SignatureReceivedEventArgs(byte[] packetBufferQueue) : base(packetBufferQueue)
        {
        }
    }
}
