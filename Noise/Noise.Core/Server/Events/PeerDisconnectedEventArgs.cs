using System;
using System.Net;

namespace Noise.Core.Server.Events
{
    public class PeerDisconnectedEventArgs : EventArgs
    {
        public EndPoint PeerEndpoint { get; set; }
    }
}
