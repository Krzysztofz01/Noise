using System;

namespace Noise.Core.Exceptions
{
    public class PacketRejectedException : Exception
    {
        public const string _invalidRsaPrivateKeyMessage = "Invalid RSA private key. Given peer is unable to decrypt given packet.";
        public const string _invalidRsaSignatureMessage = "Invalid RSA signature. Sender may try to spoof another peer.";
        public const string _invalidAesKeyMessage = "Invalid AES key. The packet may be corrupeted.";
        public const string _invalidIdentityProveMessage = "Invalid identity prove signature. Given peer is not trusted.";
        public const string _undefinedMessage = "Unexpected protocol behavior.";

        private PacketRejectedException() { }
        public PacketRejectedException(PacketRejectionReason packetRejectionReason) : base(MapReasonToMessage(packetRejectionReason)) { }

        private static string MapReasonToMessage(PacketRejectionReason packetRejectionReason)
        {
            return packetRejectionReason switch
            {
                PacketRejectionReason.INVALID_RSA_PRIVATE_KEY => _invalidRsaPrivateKeyMessage,
                PacketRejectionReason.INVALID_RSA_SIGNATURE => _invalidRsaSignatureMessage,
                PacketRejectionReason.INVALID_AES_KEY => _invalidAesKeyMessage,
                PacketRejectionReason.INVALID_IDENTITY_PROVE => _invalidIdentityProveMessage,
                _ => _undefinedMessage,
            };
        }
    }

    public enum PacketRejectionReason
    {
        INVALID_RSA_PRIVATE_KEY,
        INVALID_RSA_SIGNATURE,
        INVALID_AES_KEY,
        INVALID_IDENTITY_PROVE
    }
}
