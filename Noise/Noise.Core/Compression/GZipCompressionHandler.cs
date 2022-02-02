using System.IO;
using System.IO.Compression;

namespace Noise.Core.Compression
{
    public static class GZipCompressionHandler
    {
        public static byte[] Compress(byte[] dataBuffer)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionLevel.Optimal);
            
            zipStream.Write(dataBuffer, 0, dataBuffer.Length);
            zipStream.Close();
            
            return compressedStream.ToArray();
        }

        public static byte[] Decompress(byte[] compressedDataBuffer)
        {
            using var compressedStream = new MemoryStream(compressedDataBuffer);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            
            zipStream.CopyTo(resultStream);
            
            return resultStream.ToArray();
        }
    }
}
