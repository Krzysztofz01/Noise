using Noise.Core.Encryption;
using Xunit;

namespace Noise.Core.Test
{
    public class EncryptionTests
    {
        [Fact]
        public void AsymmetricHandlerShouldCreateKeyPair()
        {
            string privateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
            string publicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(privateKey);

            Assert.NotNull(publicKey);
            Assert.NotNull(privateKey);
        }

        [Fact]
        public void AsymmetricHandlerShouldEncryptAndDecryptUsingCorrectKeyPair()
        {
            string privateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
            string publicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(privateKey);

            string plainTextMessage = "Hello World!";

            string cipher = AsymmetricEncryptionHandler.Encrypt(plainTextMessage, publicKey);

            string decryptedCipher = AsymmetricEncryptionHandler.Decrypt(cipher, privateKey);

            Assert.Equal(plainTextMessage, decryptedCipher);
        }

        [Fact]
        public void AsymmetricHandlerShoudEncryptButNotDecryptIncorrectKeyPair()
        {
            string privateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
            string publicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(privateKey);

            string plainTextMessage = "Hello World!";

            string cipher = AsymmetricEncryptionHandler.Encrypt(plainTextMessage, publicKey);

            string incorrectPrivateKey = AsymmetricEncryptionHandler.InitializePrivateKey();

            string decryptedCipher = AsymmetricEncryptionHandler.Decrypt(cipher, incorrectPrivateKey);

            Assert.NotEqual(plainTextMessage, decryptedCipher);
            Assert.Null(decryptedCipher);
        }

        [Fact]
        public void SymmetricHandlerShouldBeCreatedAndEncryptData()
        {
            string plainTextMessage = "Hello World!";

            var (cipher, key) = SymmetricEncryptionHandler.Encrypt(plainTextMessage);

            Assert.NotNull(cipher);
            Assert.NotNull(key);
        }

        [Fact]
        public void SymmetricHandlerShouldEncryptAndDecryptUsingCorrectKey()
        {
            string plainTextMessage = "Hello World!";

            var (cipher, key) = SymmetricEncryptionHandler.Encrypt(plainTextMessage);

            var decryptedCipher = SymmetricEncryptionHandler.Decrypt(cipher, key);

            Assert.Equal(plainTextMessage, decryptedCipher);
        }

        [Fact]
        public void SymmetricHandlerShouldEncryptButNotDecryptUsingTheIncorrectKey()
        {
            string plainTextMessage = "Hello World!";
            
            var (cipher, _) = SymmetricEncryptionHandler.Encrypt(plainTextMessage);

            var wrongKey = "This is not the correct key!";

            var decryptedCipher = SymmetricEncryptionHandler.Decrypt(cipher, wrongKey);

            Assert.NotEqual(plainTextMessage, decryptedCipher);
            Assert.Null(decryptedCipher);
        }
    }
}
