using System;

namespace Noise.Core.Server.Events
{
    public class PeerDisconnectedEventArgs : EventArgs
    {
        public PeerDisconnectReason PeerDisconnectReason { get; set; }
        public string PeerEndpoint { get; set; }
    }

    public enum PeerDisconnectReason
    {
        Normal = 0,
        Timeout = 1
    }
}
