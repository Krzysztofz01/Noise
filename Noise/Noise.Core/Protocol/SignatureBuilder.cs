using Noise.Core.Hashing;
using System.Security.Cryptography;

namespace Noise.Core.Protocol
{
    public static class SignatureBuilder
    {
        private const int _signatureBufferSize = 8192;

        public static string GenerateSignature()
        {
            var signatureBuffer = new byte[_signatureBufferSize];
            RandomNumberGenerator.Fill(signatureBuffer);

            return SHA512HashingHandler.HashToBase64(signatureBuffer);
        }
    }
}
