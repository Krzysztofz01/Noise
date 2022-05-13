using System;

namespace Noise.Core.Protocol
{
    public static class Constants
    {
        public const string Version = "v0.2.0-alpha";

        public const Int32 ProtocolPort = 49490;

        public const Int32 MaximalPacketSize = 8192;

        public const Int32 ChecksumByteBufferSize = 20;

        public const Int32 PacketBaseSize = sizeof(Int32) + sizeof(Int32) + ChecksumByteBufferSize;
    }
}
