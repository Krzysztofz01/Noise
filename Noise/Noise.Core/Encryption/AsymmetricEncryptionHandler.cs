using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Noise.Core.Encryption
{
    public class AsymmetricEncryptionHandler
    {
        private const bool _useOAEPPadding = false;
        private const int _keyLength = 2048;

        private readonly RSACryptoServiceProvider _rsa = new(_keyLength);

        private readonly RSAParameters _privateKey;
        private readonly RSAParameters _publicKey;

        public AsymmetricEncryptionHandler()
        {
            _privateKey = _rsa.ExportParameters(true);
            _publicKey = _rsa.ExportParameters(false);
        }

        public AsymmetricEncryptionHandler(RSAParameters privateRsaParameters)
        {
            _rsa.ImportParameters(privateRsaParameters);

            _privateKey = _rsa.ExportParameters(true);
            _publicKey = _rsa.ExportParameters(false);
        }

        public string GetPublicKey()
        {
            using var stringWriter = new StringWriter();

            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, _publicKey);

            return stringWriter.ToString();
        }

        public string GetPrivateKey()
        {
            using var stringWriter = new StringWriter();

            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, _privateKey);

            return stringWriter.ToString();
        }

        public string Decrypt(string cypherData)
        {
            _rsa.ImportParameters(_privateKey);
            
            var dataBytes = Convert.FromBase64String(cypherData);

            try
            {
                var plainText = _rsa.Decrypt(dataBytes, _useOAEPPadding);

                return Encoding.Unicode.GetString(plainText);
            }
            catch(CryptographicException)
            {
                return null;
            }
        }

        public static string Encrypt(string plainData, string xmlSerializedPublicKey)
        {
            var xmlSerializer = new XmlSerializer(typeof(RSAParameters));

            using var stringReader = new StringReader(xmlSerializedPublicKey);

            var rsaPublicKeyParameter = (RSAParameters)xmlSerializer.Deserialize(stringReader);

            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaPublicKeyParameter);

            var encodedData = Encoding.Unicode.GetBytes(plainData);

            var cypherData = rsa.Encrypt(encodedData, _useOAEPPadding);

            return Convert.ToBase64String(cypherData);
        }
    }
}
