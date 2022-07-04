using System;

namespace Noise.Core.Protocol
{
    public enum PacketType : Int32
    {
        PING = 0,
        DISCOVERY = 1,
        DISCOVERY_FORWARD = 2,
        SIGNATURE = 3,
        MESSAGE = 4,
        KEY = 5,
        BROADCAST = 6
    }
}
