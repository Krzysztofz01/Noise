using Noise.Core.Hashing;
using System.Text;
using Xunit;

namespace Noise.Core.Test
{
    public class HashingTests
    {
        [Fact]
        public void FastHandlerShouldHashSameInputSameOutputToBytes()
        {
            var plainData = "Hello World!";
            var plainDataBytes = Encoding.UTF8.GetBytes(plainData);

            var firstHash = SHA1HashingHandler.HashToBytes(plainDataBytes);
            var secondHash = SHA1HashingHandler.HashToBytes(plainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.Equal(firstHash, secondHash);
        }

        [Fact]
        public void FastHandlerShouldHashSameInputSameOutputToBase64()
        {
            var plainData = "Hello World!";
            var plainDataBytes = Encoding.UTF8.GetBytes(plainData);

            var firstHash = SHA1HashingHandler.HashToBase64(plainDataBytes);
            var secondHash = SHA1HashingHandler.HashToBase64(plainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.Equal(firstHash, secondHash);
        }

        [Fact]
        public void FastHandlerShouldHashDifferentInputDifferentOutputToBytes()
        {
            var firstPlainData = "Hello World!";
            var firstPlainDataBytes = Encoding.UTF8.GetBytes(firstPlainData);
            var secondPlainData = "Hello again!";
            var secondPlainDataBytes = Encoding.UTF8.GetBytes(secondPlainData);

            var firstHash = SHA1HashingHandler.HashToBytes(firstPlainDataBytes);
            var secondHash = SHA1HashingHandler.HashToBytes(secondPlainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.NotEqual(firstHash, secondHash);
        }

        [Fact]
        public void FastHandlerShouldHashDifferentInputDifferentOutputToBase64()
        {
            var firstPlainData = "Hello World!";
            var firstPlainDataBytes = Encoding.UTF8.GetBytes(firstPlainData);
            var secondPlainData = "Hello again!";
            var secondPlainDataBytes = Encoding.UTF8.GetBytes(secondPlainData);

            var firstHash = SHA1HashingHandler.HashToBase64(firstPlainDataBytes);
            var secondHash = SHA1HashingHandler.HashToBase64(secondPlainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.NotEqual(firstHash, secondHash);
        }

        [Fact]
        public void LongHandlerShouldHashSameInputSameOutputToBytes()
        {
            var plainData = "Hello World!";
            var plainDataBytes = Encoding.UTF8.GetBytes(plainData);

            var firstHash = SHA512HashingHandler.HashToBytes(plainDataBytes);
            var secondHash = SHA512HashingHandler.HashToBytes(plainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.Equal(firstHash, secondHash);
        }

        [Fact]
        public void LongHandlerShouldHashSameInputSameOutputToBase64()
        {
            var plainData = "Hello World!";
            var plainDataBytes = Encoding.UTF8.GetBytes(plainData);

            var firstHash = SHA512HashingHandler.HashToBase64(plainDataBytes);
            var secondHash = SHA512HashingHandler.HashToBase64(plainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.Equal(firstHash, secondHash);
        }

        [Fact]
        public void LongHandlerShouldHashDifferentInputDifferentOutputToBytes()
        {
            var firstPlainData = "Hello World!";
            var firstPlainDataBytes = Encoding.UTF8.GetBytes(firstPlainData);
            var secondPlainData = "Hello again!";
            var secondPlainDataBytes = Encoding.UTF8.GetBytes(secondPlainData);

            var firstHash = SHA512HashingHandler.HashToBytes(firstPlainDataBytes);
            var secondHash = SHA512HashingHandler.HashToBytes(secondPlainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.NotEqual(firstHash, secondHash);
        }

        [Fact]
        public void LongHandlerShouldHashDifferentInputDifferentOutputToBase64()
        {
            var firstPlainData = "Hello World!";
            var firstPlainDataBytes = Encoding.UTF8.GetBytes(firstPlainData);
            var secondPlainData = "Hello again!";
            var secondPlainDataBytes = Encoding.UTF8.GetBytes(secondPlainData);

            var firstHash = SHA512HashingHandler.HashToBase64(firstPlainDataBytes);
            var secondHash = SHA512HashingHandler.HashToBase64(secondPlainDataBytes);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.NotEqual(firstHash, secondHash);
        }
    }
}
