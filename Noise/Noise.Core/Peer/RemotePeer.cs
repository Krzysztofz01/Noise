using System;
using System.Text.Json.Serialization;

namespace Noise.Core.Peer
{
    public class RemotePeer
    {
        [JsonIgnore]
        private const string _defaultAlias = "Anonymous";

        public string PublicKey { get; init; }
        public int Identifier { get; init; }
        public string ReceivingSignature { get; init; }

        public string Alias { get; set; }
        public string SendingSignature { get; set; }

        public void SetAlias(string alias) => Alias = alias;
        public void SetSendingSignature(string sendingSignature) => SendingSignature = sendingSignature;

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization and ,,private'' usage. Use one of the methods of the RemotePeer.Factory class.")]
        public RemotePeer() { }

        public static class Factory
        {
            public static RemotePeer FromParameters(string publicKey, int identifier, string receivingSignature, string alias = null)
            {
                #pragma warning disable CS0618 // Type or member is obsolete
                return new RemotePeer
                {
                    PublicKey = publicKey,
                    Identifier = identifier,
                    ReceivingSignature = receivingSignature,
                    Alias = alias ?? _defaultAlias
                };
                #pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
