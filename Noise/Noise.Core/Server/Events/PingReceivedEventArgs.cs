using System;

namespace Noise.Core.Server.Events
{
    public class PingReceivedEventArgs : EventArgs
    {
        public byte[] PingPacketBuffer { get; set; }
    }
}
