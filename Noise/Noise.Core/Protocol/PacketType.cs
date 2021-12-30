using System;

namespace Noise.Core.Protocol
{
    public enum PacketType : Int32
    {
        PING = 0,
        DISCOVERY = 1,
        KEY = 2,
        MESSAGE = 3
    }
}
