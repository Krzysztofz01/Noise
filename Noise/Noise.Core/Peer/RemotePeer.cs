using System;
using System.Text.Json.Serialization;

namespace Noise.Core.Peer
{
    public class RemotePeer
    {
        [JsonIgnore]
        private const string _defaultAlias = "Anonymous";

        public string PublicKey { get; init; }
        public string Alias { get; set; }
        public int Identifier { get; init; }

        public void SetAlias(string alias) => Alias = alias;

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization and ,,private'' usage. Use one of the methods of the RemotePeer.Factory class.")]
        public RemotePeer() { }

        public static class Factory
        {
            public static RemotePeer FromParameters(string publicKey, int identifier, string alias = null)
            {
                #pragma warning disable CS0618 // Type or member is obsolete
                return new RemotePeer
                {
                    PublicKey = publicKey,
                    Identifier = identifier,
                    Alias = alias ?? _defaultAlias
                };
                #pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
