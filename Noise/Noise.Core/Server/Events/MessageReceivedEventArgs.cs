using System;

namespace Noise.Core.Server.Events
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public byte[] KeyPacketBuffer { get; set; }
        public byte[] MessagePacketBuffer { get; set; }
    }
}
