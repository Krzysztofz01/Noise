using System;
using System.Security.Cryptography;

namespace Noise.Core.Hashing
{
    public static class SHA512HashingHandler
    {
        public static string HashToBase64(byte[] plainData)
        {
            var hashBytes = SHA512.HashData(plainData);

            return Convert.ToBase64String(hashBytes);
        }

        public static byte[] HashToBytes(byte[] plainData)
        {
            return SHA512.HashData(plainData);
        }
    }
}
