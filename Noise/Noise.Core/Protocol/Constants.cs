using System;

namespace Noise.Core.Protocol
{
    public static class Constants
    {
        private const Int32 NullTerminatorByteSize = 1;

        public const Int32 ProtocolPort = 5000;

        public const Int32 MaximalPacketBytesSize = 8192; //4096

        public const Int32 MinimalPacketBytesSize = 4 + NullTerminatorByteSize;

        public const Int32 MinimalPayloadStringSize = 29;
        public const Int32 MinimalPayloadBytesSize = MinimalPayloadStringSize + NullTerminatorByteSize;

        public const Int32 PublicKeyStringSize = 344;
        public const Int32 PublicKeyBytesSize = PublicKeyStringSize + NullTerminatorByteSize;
    }
}
