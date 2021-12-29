using Noise.Core.Encryption;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Xunit;

namespace Noise.Core.Test
{
    public class EncryptionTests
    {
        [Fact]
        public void HandlerShouldBeCreatedAndDeliverKeyPair()
        {
            var aeh = new AsymmetricEncryptionHandler();

            string privateKey = aeh.GetPrivateKey();
            string publicKey = aeh.GetPublicKey();

            Assert.NotNull(publicKey);
            Assert.NotNull(privateKey);
        }

        [Fact]
        public void HandlerShouldEncryptAndDecryptUsingCorrectKeyPair()
        {
            var firstAeh = new AsymmetricEncryptionHandler();
            string firstPrivateKey = firstAeh.GetPrivateKey();
            string firstPublicKey = firstAeh.GetPublicKey();

            string plainTextMessage = "Hello World!";
            string cipher = AsymmetricEncryptionHandler.Encrypt(plainTextMessage, firstPublicKey);

            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            using var stringReader = new StringReader(firstPrivateKey);
            var rsaPrivateKeyParameter = (RSAParameters)xmlSerializer.Deserialize(stringReader);

            var secondAeh = new AsymmetricEncryptionHandler(rsaPrivateKeyParameter);
            string decryptedCipher = secondAeh.Decrypt(cipher);
;
            Assert.Equal(plainTextMessage, decryptedCipher);
        }

        [Fact]
        public void HandlerShoudEncryptButNotDecryptIncorrectKeyPair()
        {
            var firstAeh = new AsymmetricEncryptionHandler();
            string firstPublicKey = firstAeh.GetPublicKey();

            string plainTextMessage = "Hello World!";
            string cipher = AsymmetricEncryptionHandler.Encrypt(plainTextMessage, firstPublicKey);

            var secondAeh = new AsymmetricEncryptionHandler();
            string decryptedCipher = secondAeh.Decrypt(cipher);

            Assert.NotEqual(plainTextMessage, decryptedCipher);
        }
    }
}
