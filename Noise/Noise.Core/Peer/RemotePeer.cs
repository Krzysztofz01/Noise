using Noise.Core.Extensions;
using Noise.Core.Peer.Persistence;
using System;

namespace Noise.Core.Peer
{
    public class RemotePeer
    {
        private const string _defaultAlias = "Anonymous";

        public int Identifier { get; private set; }
        public string PublicKey { get; private set; }
        public string Alias { get; private set; }
        public string ReceivingSignature { get; private set; }
        public string SendingSignature { get; private set; }

        public void SetAlias(string alias)
        {
            if (alias.IsEmpty())
                throw new ArgumentNullException(nameof(alias), "Invalid alias value for peer.");

            Alias = alias;
        }

        public void SetReceivingSignature(string receivingSignature) => ReceivingSignature = receivingSignature;
        public void SetSendingSignature(string sendingSignature) => SendingSignature = sendingSignature;

        public RemotePeerPersistence Serialize() 
        {
            return new RemotePeerPersistence
            {
                Identifier = Identifier,
                PublicKey = PublicKey,
                Alias = Alias,
                ReceivingSignature = ReceivingSignature,
                SendingSignature = SendingSignature
            };
        }

        private RemotePeer() { }
        public static class Factory
        {
            public static RemotePeer FromParameters(string publicKey, int identifier, string receivingSignature, string alias = null)
            {
                if (publicKey.IsEmpty())
                    throw new ArgumentNullException(nameof(publicKey), "Invalid public key for peer.");

                if (alias is not null && alias.Trim() == string.Empty)
                    throw new ArgumentNullException(nameof(alias), "Invalid alias value for peer.");

                return new RemotePeer
                {
                    Identifier = identifier,
                    PublicKey = publicKey,
                    Alias = alias ?? _defaultAlias,
                    ReceivingSignature = receivingSignature,
                    SendingSignature = null
                };
            }

            public static RemotePeer Deserialize(RemotePeerPersistence remotePeer)
            {
                return new RemotePeer
                {
                    Identifier = remotePeer.Identifier,
                    PublicKey = remotePeer.PublicKey,
                    Alias = remotePeer.Alias,
                    ReceivingSignature = remotePeer.ReceivingSignature,
                    SendingSignature = remotePeer.SendingSignature
                };
            }
        }
    }
}
