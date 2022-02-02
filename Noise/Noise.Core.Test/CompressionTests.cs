using Noise.Core.Compression;
using System.Security.Cryptography;
using Xunit;

namespace Noise.Core.Test
{
    public class CompressionTests
    {
        [Fact]
        public void DataShouldCompressAndDecompress()
        {
            var data = new byte[16384];
            RandomNumberGenerator.Fill(data);

            var compressedData = GZipCompressionHandler.Compress(data);

            var decompressedData = GZipCompressionHandler.Decompress(compressedData);

            Assert.Equal(data, decompressedData);
        }
    }
}
