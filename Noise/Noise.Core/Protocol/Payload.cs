using Noise.Core.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noise.Core.Protocol
{
    public class Payload
    {
        private readonly string _publicKey;
        public string PublicKey
        {
            get => _publicKey.IsEmpty()
                ? throw new InvalidOperationException("This payload does not contain a public key. Make sure you perform operations on the correct type of packet.")
                : _publicKey;

            init => _publicKey = value ?? string.Empty;
        }

        public readonly string _content;
        public string Content
        {
            get => _content;
            init => _content = value;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                IncludeFields = false
            });
        }
        private Payload() { }

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization. Use one of the methods of the Payload.Factory class.")]
        public Payload(string publicKey, string content)
        {
            PublicKey = publicKey;
            Content = content;
        }

        public static class Factory
        {
            public static Payload Deserialize(string serializedPayload, bool validatePublicKeyPresence = true)
            {
                if (serializedPayload.IsEmpty())
                    throw new ArgumentNullException("Invalid payload format. The packet may be corrupted.");

                var payload = JsonSerializer.Deserialize<Payload>(serializedPayload, new JsonSerializerOptions
                {
                    IgnoreReadOnlyFields = true,
                    IgnoreReadOnlyProperties = true,
                    IncludeFields = false
                });

                if (payload.PublicKey.IsEmpty() && validatePublicKeyPresence)
                    throw new ArgumentException("The public key is required. Make sure to use correct valiation rules and packet type.");

                return payload;
            }

            public static Payload FromParameters(string content)
            {
                return new Payload
                {
                    PublicKey = null,
                    Content = content.IsEmpty() ? string.Empty : content
                };
            }

            public static Payload FromParameters(string publicKey, string content, bool validatePublicKeyPresence = true)
            {
                if (publicKey.IsEmpty() && validatePublicKeyPresence)
                    throw new ArgumentException("The public key is required. Make sure to use correct valiation rules and packet type.");

                if (publicKey.Length != Constants.PublicKeyStringSize)
                    throw new ArgumentException("The public key format is invalid.");

                return new Payload
                {
                    PublicKey = publicKey,
                    Content = content.IsEmpty() ? string.Empty : content
                };
            }

            public static Payload Empty => FromParameters(string.Empty);
        }
    }
}
