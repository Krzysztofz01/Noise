using Noise.Core.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noise.Core.Encryption
{
    public static class AsymmetricEncryptionHandler
    {
        private const bool _useOAEPPadding = false;
        private const int _keyLength = 4096;
        private static readonly byte[] _AQAB = new byte[] { 1, 0, 1 };

        private static readonly JsonSerializerOptions _serializationOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            IncludeFields = true,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public static string InitializePrivateKey()
        {
            using var rsa = new RSACryptoServiceProvider(_keyLength);

            var privateKeyStruct = rsa.ExportParameters(true);

            var serializedPrivateKeyStruct = JsonSerializer.Serialize(privateKeyStruct, _serializationOptions);

            return serializedPrivateKeyStruct.FromUtf8ToBase64();
        }

        public static string GetPublicKeyBase64(string privateKeyBase64)
        {
            var serializedPrivateKey = privateKeyBase64.FromBase64ToUtf8();

            var privateKey = JsonSerializer.Deserialize<RSAParameters>(serializedPrivateKey, _serializationOptions);

            return Convert.ToBase64String(privateKey.Modulus);
        }

        public static string Decrypt(string cipherDataBase64, string privateKeyBase64)
        {
            var serializedPrivateKey = privateKeyBase64.FromBase64ToUtf8();

            var privateKey = JsonSerializer.Deserialize<RSAParameters>(serializedPrivateKey, _serializationOptions);

            using var rsa = new RSACryptoServiceProvider(_keyLength);
            rsa.ImportParameters(privateKey);

            var dataBytes = Convert.FromBase64String(cipherDataBase64);

            try
            {
                var plainData = rsa.Decrypt(dataBytes, _useOAEPPadding);

                return Encoding.UTF8.GetString(plainData);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public static string Encrypt(string plainData, string publicKeyBase64)
        {
            var rsaParameters = new RSAParameters
            {
                Modulus = Convert.FromBase64String(publicKeyBase64),
                Exponent = _AQAB
            };

            using var rsa = new RSACryptoServiceProvider(_keyLength);
            rsa.ImportParameters(rsaParameters);

            var encodedData = Encoding.UTF8.GetBytes(plainData);

            var cipherData = rsa.Encrypt(encodedData, _useOAEPPadding);

            return Convert.ToBase64String(cipherData);
        }
    }
}
