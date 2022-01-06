using Noise.Core.Protocol;
using System;
using System.Net.Sockets;

namespace Noise.Core.Events
{
    public class PacketPairReceivedEventsArgs : EventArgs
    {
        public TcpClient Client { get; set; }
        public Packet KeyPacket { get; set; }
        public Packet CipherPacket { get; set; }
    }
}
