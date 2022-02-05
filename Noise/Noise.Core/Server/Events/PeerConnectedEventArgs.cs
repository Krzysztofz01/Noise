using System;

namespace Noise.Core.Server.Events
{
    public class PeerConnectedEventArgs : EventArgs
    {
        public string PeerEndpoint { get; set; }
    }
}
