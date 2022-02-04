using System;

namespace Noise.Core.Server.Events
{
    public class DiscoveryReceivedEventArgs : EventArgs
    {
        public byte[] KeyPacketBuffer { get; set; }
        public byte[] DiscoveryPacketBuffer { get; set; }
    }
}
