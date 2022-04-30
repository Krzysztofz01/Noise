using Noise.Core.Encryption;
using Noise.Core.Extensions;
using Noise.Core.Peer.Persistence;
using System;

namespace Noise.Core.Peer
{
    public class PeerSecrets
    {
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }
        public string ConfigurationSecret { get; private set; }

        public PeerSecretsPersistence Serialize()
        {
            return new PeerSecretsPersistence
            {
                PublicKey = PublicKey,
                PrivateKey = PrivateKey,
                ConfigurationSecret = ConfigurationSecret
            };
        }

        private PeerSecrets() { }
        public static class Factory
        {
            public static PeerSecrets Initialize(string configurationSecret)
            {
                if (configurationSecret.IsEmpty())
                    throw new ArgumentException("Invalid configuration secret.", nameof(configurationSecret));

                var privateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
                var publicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(privateKey);

                return new PeerSecrets
                {
                    PrivateKey = privateKey,
                    PublicKey = publicKey,
                    ConfigurationSecret = configurationSecret
                };
            }

            public static PeerSecrets Deserialize(PeerSecretsPersistence peerSecrets)
            {
                return new PeerSecrets
                {
                    PrivateKey = peerSecrets.PrivateKey,
                    PublicKey = peerSecrets.PublicKey,
                    ConfigurationSecret = peerSecrets.ConfigurationSecret
                };
            }
        }
    }
}
