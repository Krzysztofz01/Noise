using Noise.Core.Hashing;
using Xunit;

namespace Noise.Core.Test
{
    public class HashingTests
    {
        [Fact]
        public void FastHandlerShouldHashSameInputSameOutput()
        {
            var plainData = "Hello World!";

            var firstHash = FastHashingHandler.Hash(plainData);
            var secondHash = FastHashingHandler.Hash(plainData);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.Equal(firstHash, secondHash);
        }

        [Fact]
        public void FastHandlerShouldHashDifferentInputDifferentOutput()
        {
            var firstPlainData = "Hello World!";
            var secondPlainData = "Hello again!";

            var firstHash = FastHashingHandler.Hash(firstPlainData);
            var secondHash = FastHashingHandler.Hash(secondPlainData);

            Assert.NotNull(firstHash);
            Assert.NotNull(secondHash);

            Assert.NotEqual(firstHash, secondHash);
        }
    }
}
