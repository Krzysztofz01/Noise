using Noise.Core.Encryption;
using Noise.Core.Extensions;

namespace Noise.Core.Peer
{
    public static class PeerEncryption
    {
        public static PeerConfiguration DecryptPeerConfiguration(string peerCipher, string peerPassword)
        {
            var serializedConfiguration = SymmetricEncryptionHandler.Decrypt(peerCipher, peerPassword.FromUtf8ToBase64());
            if (serializedConfiguration is null) return null;

            return PeerConfiguration.Factory.Deserialize(serializedConfiguration);
        }

        public static string EncryptPeerConfiguration(PeerConfiguration peerConfiguration)
        {
            return SymmetricEncryptionHandler.Encrypt(peerConfiguration.Serialize(), peerConfiguration.ConfigurationSecret.FromUtf8ToBase64());
        }
    }
}
