using System;

namespace Noise.Core.Extensions
{
    internal static class Int32Extensions
    {
        public static byte[] ToLowEndianByteBuffer(this Int32 value)
        {
            byte[] data = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian) Array.Reverse(data);

            return data;
        }
    }
}
