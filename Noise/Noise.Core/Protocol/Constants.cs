using System;

namespace Noise.Core.Protocol
{
    public static class Constants
    {   
        public const Int32 ProtocolPort = 5000;

        public const Int32 MaximalPacketSize = 8192;

        public const Int32 ChecksumByteBufferSize = 20;

        public const Int32 PacketBaseSize = sizeof(Int32) + sizeof(Int32) + ChecksumByteBufferSize;
    }
}
