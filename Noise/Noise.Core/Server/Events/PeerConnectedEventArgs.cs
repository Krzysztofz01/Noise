using System;
using System.Net;

namespace Noise.Core.Server.Events
{
    public class PeerConnectedEventArgs : EventArgs
    {
        public EndPoint PeerEndpoint { get; set; }
    }
}
