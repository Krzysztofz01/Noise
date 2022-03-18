using Noise.Core.Extensions;
using Noise.Core.Hashing;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noise.Core.Encryption
{
    public static class AsymmetricSignatureHandler
    {
        private const string _hashAlgorithmName = "SHA512";
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

        public static string GetSignatureBase64(string inputDataBase64, string senderPrivateKeyBase64)
        {
            var serializedPrivateKey = senderPrivateKeyBase64.FromBase64ToUtf8();

            var privateKeyParameters = JsonSerializer.Deserialize<RSAParameters>(serializedPrivateKey, _serializationOptions);

            using var rsa = RSA.Create(privateKeyParameters);

            var inputDataHash = SHA512HashingHandler.HashToBytes(inputDataBase64.FromBase64ToByteBuffer());

            var rsaPkcsFormatter = new RSAPKCS1SignatureFormatter(rsa);
            rsaPkcsFormatter.SetHashAlgorithm(_hashAlgorithmName);

            var signedHash = rsaPkcsFormatter.CreateSignature(inputDataHash);

            return signedHash.FromByteBufferToBase64();
        }

        public static bool VerifySignature(string inputDataBase64, string signatureBase64, string senderPublicKeyBase64)
        {
            var publicKeyBuffer = senderPublicKeyBase64.FromBase64ToByteBuffer();
            var signatureBuffer = signatureBase64.FromBase64ToByteBuffer();

            var rsaParameters = new RSAParameters
            {
                Modulus = publicKeyBuffer,
                Exponent = _AQAB
            };

            var inputDataHash = SHA512HashingHandler.HashToBytes(inputDataBase64.FromBase64ToByteBuffer());

            var rsaPkcsDeformatter = new RSAPKCS1SignatureDeformatter(RSA.Create(rsaParameters));
            rsaPkcsDeformatter.SetHashAlgorithm(_hashAlgorithmName);

            return rsaPkcsDeformatter.VerifySignature(inputDataHash, signatureBuffer);
        }
    }
}
