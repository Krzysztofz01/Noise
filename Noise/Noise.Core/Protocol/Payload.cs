using Noise.Core.Extensions;
using System;
using System.Text;

namespace Noise.Core.Protocol
{
    public class Payload
    {
        public string PublicKey { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(PublicKey);
            sb.Append(Content);

            return sb.ToString();
        }

        private Payload() { }

        public static class Factory
        {
            public static Payload FromString(string payload, bool validateLength = true)
            {
                if (payload.Length < Constants.PublicKeySize && validateLength)
                    throw new ArgumentException("Invalid payload size. Length verification only makes sense for MSG packets.");

                var publicKey = payload.Substring(0, Constants.PublicKeySize);
                var content = payload.Substring(Constants.PublicKeySize);

                return FromParameters(publicKey, content);
            }

            public static Payload FromParameters(string publicKey, string content, bool validateLength = true)
            {
                if (publicKey.Length < Constants.PublicKeySize && validateLength)
                    throw new ArgumentException("Invalid payload size. Length verification only makes sense for MSG packets.");

                return new Payload
                {
                    PublicKey = publicKey.IsEmpty() ? string.Empty : publicKey,
                    Content = content.IsEmpty() ? string.Empty : content
                };
            }

            public static Payload Empty => FromParameters(string.Empty, string.Empty, false);
        }
    }
}
