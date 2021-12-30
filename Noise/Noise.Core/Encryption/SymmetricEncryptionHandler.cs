using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Noise.Core.Encryption
{
    public class SymmetricEncryptionHandler
    {
        private const int _keySize = 64;
        private const int _saltSize = 8;
        private const int _bytesDeriveIterations = 1000;

        public SymmetricEncryptionHandler() { }

        public (string cipher, string key) Encrypt(string data)
        {
            var keyBytes = new byte[_keySize];
            
            using var rngCrypto = new RNGCryptoServiceProvider();
            rngCrypto.GetBytes(keyBytes);

            var encodedKey = Convert.ToBase64String(keyBytes);

            using var aesGcm = CreateAesGcm(encodedKey);

            byte[] dataBytes = Encoding.Unicode.GetBytes(data);

            int nonceSize = AesGcm.NonceByteSizes.MaxSize;
            int tagSize = AesGcm.TagByteSizes.MaxSize;
            int cipherSize = dataBytes.Length;

            int encryptedDataLength = 4 + nonceSize + 4 + tagSize + cipherSize;

            Span<byte> encryptedData = encryptedDataLength < 1024
                ? stackalloc byte[encryptedDataLength]
                : new byte[encryptedDataLength].AsSpan();

            BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(0, 4), nonceSize);
            BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4), tagSize);

            var nonce = encryptedData.Slice(4, nonceSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            RandomNumberGenerator.Fill(nonce);

            aesGcm.Encrypt(nonce, dataBytes.AsSpan(), cipherBytes, tag);

            var encodedCipher = Convert.ToBase64String(encryptedData);

            return (encodedCipher, encodedKey);
        }
        
        public string Decrypt(string cipher, string key)
        {
            Span<byte> encryptedData = Convert.FromBase64String(cipher).AsSpan();

            using var aesGcm = CreateAesGcm(key);

            int nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(0, 4));
            int tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4));
            int cipherSize = encryptedData.Length - 4 - nonceSize - 4 - tagSize;

            var nonce = encryptedData.Slice(4, nonceSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            Span<byte> plainBytes = cipherSize < 1024
                ? stackalloc byte[cipherSize]
                : new byte[cipherSize];

            try
            {
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

                return Encoding.Unicode.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        private AesGcm CreateAesGcm(string key)
        {
            var salt = new byte[_saltSize];

            byte[] keyBytes = new Rfc2898DeriveBytes(key, salt, _bytesDeriveIterations).GetBytes(16);
            return new AesGcm(keyBytes);
        }
    }
}
