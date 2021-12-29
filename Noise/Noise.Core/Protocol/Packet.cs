using System;
using System.Text;

namespace Noise.Core.Protocol
{
    public abstract class Packet
    {
        public abstract string Digest { get; }
        public string Content { get; private set; }

        protected Packet()
        {
            Content = string.Empty;
        }

        protected void SetContent(string content)
        {
            int contentSizeInBytes = Encoding.UTF8.GetByteCount(content);

            if (contentSizeInBytes > Constants.PacketContentMaxSize)
                throw new ArgumentOutOfRangeException(nameof(content), "The content size is to big.");

            Content = content;
        }

        public virtual byte[] EncodePacket()
        {
            string payload = $"{Digest}_{Content}";

            return Encoding.UTF8.GetBytes(payload);
        }
    }
}
