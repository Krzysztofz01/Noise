using System;
using System.Net;

namespace Noise.Core.Events
{
    public class ClientDisconnectedEventsArgs : EventArgs
    {
        public EndPoint Endpoint { get; set; }
    }
}
