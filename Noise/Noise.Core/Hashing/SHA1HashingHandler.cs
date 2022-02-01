using System;
using System.Security.Cryptography;

namespace Noise.Core.Hashing
{
    public static class SHA1HashingHandler
    {
        public static string HashToBase64(byte[] plainData)
        {
            var hashBytes = SHA1.HashData(plainData);

            return Convert.ToBase64String(hashBytes);
        }

        public static byte[] HashToBytes(byte[] plainData)
        {
            return SHA1.HashData(plainData);
        }
    }
}
