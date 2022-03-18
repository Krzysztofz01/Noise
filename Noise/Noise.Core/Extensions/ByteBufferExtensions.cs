using System;

namespace Noise.Core.Extensions
{
    internal static class ByteBufferExtensions
    {
        public static Int32 ToInt32(this byte[] buffer, int offset)
        {
            byte[] bufferCopy = new byte[buffer.Length];
            buffer.CopyTo(bufferCopy, 0);

            if (!BitConverter.IsLittleEndian) Array.Reverse(bufferCopy);

            return BitConverter.ToInt32(bufferCopy, offset);
        }

        public static string FromByteBufferToBase64(this byte[] buffer)
        {
            return Convert.ToBase64String(buffer);
        }
    }
}
