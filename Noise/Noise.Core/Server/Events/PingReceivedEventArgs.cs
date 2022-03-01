using System;

namespace Noise.Core.Server.Events
{
    public class PingReceivedEventArgs : EventArgs
    {
        public string PeerEndpoint { get; set; }

        public PingReceivedEventArgs(string peerEndpoint) =>
            PeerEndpoint = peerEndpoint;
    }
}
