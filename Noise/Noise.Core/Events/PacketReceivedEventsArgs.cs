using Noise.Core.Protocol;
using System;
using System.Net.Sockets;

namespace Noise.Core.Events
{
    public class PacketReceivedEventsArgs : EventArgs
    {
        public TcpClient Client { get; set; }
        public Packet Packet { get; set; }
    }
}
