using System;

namespace Noise.Core.Server.Events
{
    public class SignatureReceivedEventArgs : EventArgs
    {
        public byte[] SignaturePacketBuffer { get; set; }
    }
}
