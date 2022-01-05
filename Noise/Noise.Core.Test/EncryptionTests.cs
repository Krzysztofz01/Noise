using Noise.Core.Encryption;
using Xunit;

namespace Noise.Core.Test
{
    public class EncryptionTests
    {
        [Fact]
        public void AsymmetricHandlerShouldBeCreatedAndDeliverKeyPair()
        {
            var aeh = new AsymmetricEncryptionHandler();

            string privateKey = aeh.GetPrivateKey();
            string publicKey = aeh.GetPublicKey();

            Assert.NotNull(publicKey);
            Assert.NotNull(privateKey);
        }

        [Fact]
        public void AsymmetricHandlerShouldEncryptAndDecryptUsingCorrectKeyPair()
        {
            var firstAeh = new AsymmetricEncryptionHandler();
            string firstPrivateKey = firstAeh.GetPrivateKey();
            string firstPublicKey = firstAeh.GetPublicKey();

            string plainTextMessage = "Hello World!";
            
            string cipher = AsymmetricEncryptionHandler.Encrypt(plainTextMessage, firstPublicKey);

            var secondAeh = new AsymmetricEncryptionHandler(firstPrivateKey);
            string decryptedCipher = secondAeh.Decrypt(cipher);

            Assert.Equal(plainTextMessage, decryptedCipher);
        }

        [Fact]
        public void AsymmetricHandlerShoudEncryptButNotDecryptIncorrectKeyPair()
        {
            var firstAeh = new AsymmetricEncryptionHandler();
            string firstPublicKey = firstAeh.GetPublicKey();

            string plainTextMessage = "Hello World!";
            string cipher = AsymmetricEncryptionHandler.Encrypt(plainTextMessage, firstPublicKey);

            var secondAeh = new AsymmetricEncryptionHandler();
            string decryptedCipher = secondAeh.Decrypt(cipher);

            Assert.NotEqual(plainTextMessage, decryptedCipher);
        }

        [Fact]
        public void SymmetricHandlerShouldBeCreatedAndEncryptData()
        {
            var seh = new SymmetricEncryptionHandler();

            string plainTextMessage = "Hello World!";
            var (cipher, key) = seh.Encrypt(plainTextMessage);

            Assert.NotNull(cipher);
            Assert.NotNull(key);
        }

        [Fact]
        public void SymmetricHandlerShouldEncryptAndDecryptUsingCorrectKey()
        {
            var firstSeh = new SymmetricEncryptionHandler();

            string plainTextMessage = "Hello World!";
            var (cipher, key) = firstSeh.Encrypt(plainTextMessage);

            int keyLength = key.Length;

            var secondSeh = new SymmetricEncryptionHandler();
            var decryptedCipher = secondSeh.Decrypt(cipher, key);

            Assert.Equal(plainTextMessage, decryptedCipher);
        }

        [Fact]
        public void SymmetricHandlerShouldEncryptButNotDecryptUsingTheIncorrectKey()
        {
            var firstSeh = new SymmetricEncryptionHandler();

            string plainTextMessage = "Hello World!";
            var (cipher, key) = firstSeh.Encrypt(plainTextMessage);

            var secondSeh = new SymmetricEncryptionHandler();
            var wrongKey = "This is not the correct key!";
            var decryptedCipher = secondSeh.Decrypt(cipher, wrongKey);

            Assert.NotEqual(plainTextMessage, decryptedCipher);
        }
    }
}
