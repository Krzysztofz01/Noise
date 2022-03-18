using Noise.Core.Encryption;
using Noise.Core.Extensions;
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
        public void AsymmetricSignatureHandlerShouldGenerateAndVerifyCorrectSignature()
        {
            string privateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
            string publicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(privateKey);

            string plainTextMessage = "Hello World!";
            string plainTextMessageBase64 = plainTextMessage.FromUtf8ToBase64();

            var signature = AsymmetricSignatureHandler.GetSignatureBase64(plainTextMessageBase64, privateKey);

            var verificationResult = AsymmetricSignatureHandler.VerifySignature(plainTextMessageBase64, signature, publicKey);

            Assert.True(verificationResult);
        }

        [Fact]
        public void AsymmetricSignatureHandlerShouldGenerateAndNotVerifyInvalidSignature()
        {
            string firstPrivateKey = AsymmetricEncryptionHandler.InitializePrivateKey();

            string secondPrivateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
            string secondPublicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(secondPrivateKey);

            string plainTextMessage = "Hello World!";
            string plainTextMessageBase64 = plainTextMessage.FromUtf8ToBase64();

            var signature = AsymmetricSignatureHandler.GetSignatureBase64(plainTextMessageBase64, firstPrivateKey);

            var verificationResult = AsymmetricSignatureHandler.VerifySignature(plainTextMessageBase64, signature, secondPublicKey);

            Assert.False(verificationResult);
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
        public void SymmetricHandlerShouldBeCreatedAndEncryptDataWithExplicitKey()
        {
            string plainTextMessage = "Hello World!";
            
            string plainTextKey = "Hello World!";
            string base64Key = plainTextKey.FromUtf8ToBase64();

            var cipher = SymmetricEncryptionHandler.Encrypt(plainTextMessage, base64Key);

            Assert.NotNull(cipher);
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
        public void SymmetricHandlerShouldEncryptAndDecryptUsingCorrectExplicitKey()
        {
            string plainTextMessage = "Hello World!";

            string plainTextKey = "Hello World!";
            string base64Key = plainTextKey.FromUtf8ToBase64();

            var cipher = SymmetricEncryptionHandler.Encrypt(plainTextMessage, base64Key);

            var decryptedCipher = SymmetricEncryptionHandler.Decrypt(cipher, base64Key);

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

        [Fact]
        public void SymmetricHandlerShouldEncryptButNotDecryptUsingTheIncorrectExplicitKey()
        {
            string plainTextMessage = "Hello World!";

            string plainTextKey = "Hello World!";
            string base64Key = plainTextKey.FromUtf8ToBase64();

            var cipher = SymmetricEncryptionHandler.Encrypt(plainTextMessage, base64Key);

            var wrongKey = "This is not the correct key!";

            var decryptedCipher = SymmetricEncryptionHandler.Decrypt(cipher, wrongKey);

            Assert.NotEqual(plainTextMessage, decryptedCipher);
            Assert.Null(decryptedCipher);
        }
    }
}
