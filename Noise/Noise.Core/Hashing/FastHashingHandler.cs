using System;
using System.Security.Cryptography;
using System.Text;

namespace Noise.Core.Hashing
{
    public static class FastHashingHandler
    {
        public static string Hash(string plainData)
        {
            var dataHash = SHA1.HashData(Encoding.UTF8.GetBytes(plainData));

            return Convert.ToBase64String(dataHash);
        }
    }
}
