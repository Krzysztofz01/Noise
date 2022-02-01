using System;

namespace Noise.Core.Protocol
{
    public static class Constants
    {   
        /*//private const Int32 NullTerminatorByteSize = 1;

        //public const Int32 ProtocolPort = 5000;

        //public const Int32 MaximalPacketBytesSize = 8192; //4096

        public const Int32 MinimalPacketBytesSize = 4 + NullTerminatorByteSize;

        public const Int32 MinimalPayloadStringSize = 29;
        public const Int32 MinimalPayloadBytesSize = MinimalPayloadStringSize + NullTerminatorByteSize;

        public const Int32 PublicKeyStringSize = 684; //344
        public const Int32 PublicKeyBytesSize = PublicKeyStringSize + NullTerminatorByteSize;
        // ===
        
        //public const Int32 ProtocolPort = 5000;

        private const Int32 NullTerminatorByteSize = 1;

        public const Int32 MinimalSerializedPayloadStringSize = 0;
        public const Int32 MinimalSerializedPayloadByteSize = MinimalSerializedPayloadStringSize + NullTerminatorByteSize;

        public const Int32 MaximalSerializedPayloadStringSize = 8154;
        public const Int32 MaximalSerializedPayloadByteSize = MaximalSerializedPayloadStringSize + NullTerminatorByteSize;

        public const Int32 ChecksumStringSize = 28;
        public const Int32 ChecksumByteSize = ChecksumStringSize + NullTerminatorByteSize;

        public const Int32 MinimalParcelByteSize = sizeof(Int32) + sizeof(Int32) + MinimalSerializedPayloadByteSize + ChecksumByteSize;
        public const Int32 MaximalParcelBytesSize = 8192;

        // ================*/

        public const Int32 ProtocolPort = 5000;

        public const Int32 MaximalPacketSize = 8192;

        public const Int32 ChecksumByteBufferSize = 20;

        public const Int32 PacketBaseSize = sizeof(Int32) + sizeof(Int32) + ChecksumByteBufferSize;
    }
}
