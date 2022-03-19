using Noise.Core.Abstraction;
using Noise.Core.Encryption;
using Noise.Core.Exceptions;
using Noise.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Noise.Core.Protocol
{
    public class PacketHandlingService : IPacketHandlingService
    {
        public (IPacket keyPacket, IPacket discoveryPacket) CreateDiscoveryPackets(string senderIdentityProve, string receiverPublicKey, IEnumerable<string> endpoints, IEnumerable<string> publicKeys)
        {
            var serializedEndpoints = JsonSerializer.Serialize(endpoints);
            var serializedPublicKeys = JsonSerializer.Serialize(publicKeys);

            var (endpointsCipher, endpointsKey) = SymmetricEncryptionHandler.Encrypt(serializedEndpoints);
            var (publicKeysCipher, publicKeysKey) = SymmetricEncryptionHandler.Encrypt(serializedPublicKeys);
            var (signatureCipher, signatureKey) = SymmetricEncryptionHandler.Encrypt(senderIdentityProve);

            var discoveryKey = $"{endpointsKey},{publicKeysKey}";

            var discoveryPayload = DiscoveryPayload.Factory.Create(publicKeysCipher, endpointsCipher, signatureCipher);
            var discoveryPacket = Packet<DiscoveryPayload>.Factory.FromPayload(discoveryPayload);

            var discoveryKeyCipher = AsymmetricEncryptionHandler.Encrypt(discoveryKey, receiverPublicKey);
            var signatureKeyCipher = AsymmetricEncryptionHandler.Encrypt(signatureKey, receiverPublicKey);

            var keyPayload = KeyPayload.Factory.Create(discoveryKeyCipher, signatureKeyCipher);
            var keyPacket = Packet<KeyPayload>.Factory.FromPayload(keyPayload);

            return (keyPacket, discoveryPacket);
        }

        public (IPacket keyPacket, IPacket messagePacket) CreateMessagePackets(string senderIdentityProve, string receiverPublicKey, string message)
        {
            var (messageCipher, messageKey) = SymmetricEncryptionHandler.Encrypt(message);
            var (signatureCipher, signatureKey) = SymmetricEncryptionHandler.Encrypt(senderIdentityProve);

            var messagePayload = MessagePayload.Factory.Create(messageCipher, signatureCipher);
            var messagePacket = Packet<MessagePayload>.Factory.FromPayload(messagePayload);

            var messageKeyCipher = AsymmetricEncryptionHandler.Encrypt(messageKey, receiverPublicKey);
            var signatureKeyCipher = AsymmetricEncryptionHandler.Encrypt(signatureKey, receiverPublicKey);

            var keyPayload = KeyPayload.Factory.Create(messageKeyCipher, signatureKeyCipher);
            var keyPacket = Packet<KeyPayload>.Factory.FromPayload(keyPayload);

            return (keyPacket, messagePacket);
        }

        public IPacket CreatePingPacket()
        {
            var pingPayload = PingPayload.Factory.Create();
            var pingPacket = Packet<PingPayload>.Factory.FromPayload(pingPayload);

            return pingPacket;
        }

        public (IPacket keyPacket, IPacket signaturePacket, string receiverIdentityProve) CreateSignaturePacket(string receiverPublicKey, string senderPublicKey, string senderPrivateKey, string certification = null)
        {
            var signature = SignatureBuilder.GenerateSignature();
            var asymmetricSignature = AsymmetricSignatureHandler.GetSignatureBase64(signature.FromUtf8ToBase64(), senderPrivateKey);

            var (signatureCipher, signatureKey) = SymmetricEncryptionHandler.Encrypt(signature);
            var (senderPublicKeyCipher, senderPublicKeyKey) = SymmetricEncryptionHandler.Encrypt(senderPublicKey);
            var (asymmetricSignatureCipher, asymmetricSignatureKey) = SymmetricEncryptionHandler.Encrypt(asymmetricSignature);
            var (certificationCipher, certificationKey) = SymmetricEncryptionHandler.Encrypt(certification ?? string.Empty);

            var signatureKeys = $"{signatureKey},{senderPublicKeyKey},{asymmetricSignatureKey},{certificationKey}";

            var signaturePayload = SignaturePayload.Factory.Create(signatureCipher, senderPublicKeyCipher, asymmetricSignatureCipher, certificationCipher);
            var signaturePacket = Packet<SignaturePayload>.Factory.FromPayload(signaturePayload);

            var signatureKeyCipher = AsymmetricEncryptionHandler.Encrypt(signatureKeys, receiverPublicKey);

            var keyPayload = KeyPayload.Factory.Create(signatureKeyCipher, string.Empty);
            var keyPacket = Packet<KeyPayload>.Factory.FromPayload(keyPayload);

            return (keyPacket, signaturePacket, signature);
        }

        public (IEnumerable<string> endpoints, IEnumerable<string> publicKeys, string senderIdentityProve) ReceiveDiscoveryCollections(byte[] keyPacketBuffer, byte[] discoveryPacketBuffer, string receiverPrivateKey)
        {
            var keyPayload = Packet.Factory.FromBuffer<KeyPayload>(keyPacketBuffer).PeekPayload;
            var discoveryPayload = Packet.Factory.FromBuffer<DiscoveryPayload>(discoveryPacketBuffer).PeekPayload;

            var discoveryKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.MessageKey, receiverPrivateKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_RSA_PRIVATE_KEY);
            var signatureKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.IdentityProveKey, receiverPrivateKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_RSA_PRIVATE_KEY);

            var endpointsKey = discoveryKey.Split(',').First();
            var publicKeysKey = discoveryKey.Split(',').Last();

            var signature = SymmetricEncryptionHandler.Decrypt(discoveryPayload.IdentityProve, signatureKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);
            var endpoints = SymmetricEncryptionHandler.Decrypt(discoveryPayload.Endpoints, endpointsKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);
            var publicKeys = SymmetricEncryptionHandler.Decrypt(discoveryPayload.PublicKeys, publicKeysKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);

            var deserializedEndpoints = JsonSerializer.Deserialize<IEnumerable<string>>(endpoints);
            var deserializedPublicKeys = JsonSerializer.Deserialize<IEnumerable<string>>(publicKeys);

            return (deserializedEndpoints, deserializedPublicKeys, signature);
        }

        public (string signature, string senderPublicKey, string certification) ReceiveIdentityProve(byte[] keyPacketBuffer, byte[] signaturePacketBuffer, string receiverPrivateKey)
        {
            var keyPayload = Packet.Factory.FromBuffer<KeyPayload>(keyPacketBuffer).PeekPayload;
            var signaturePayload = Packet.Factory.FromBuffer<SignaturePayload>(signaturePacketBuffer).PeekPayload;

            var signatureKeys = AsymmetricEncryptionHandler.Decrypt(keyPayload.MessageKey, receiverPrivateKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_RSA_PRIVATE_KEY);

            var signatureKey = signatureKeys.Split(',').First();
            var senderPublicKeyKey = signatureKeys.Split(',').Skip(1).First();
            var asymmetricSignatureKey = signatureKeys.Split(',').Skip(2).First();
            var certificationKey = signatureKeys.Split(',').Last();

            var signature = SymmetricEncryptionHandler.Decrypt(signaturePayload.Signature, signatureKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);
            var senderPublicKey = SymmetricEncryptionHandler.Decrypt(signaturePayload.SenderPublicKey, senderPublicKeyKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);
            var asymmetricSignature = SymmetricEncryptionHandler.Decrypt(signaturePayload.SenderAsymmetricSignature, asymmetricSignatureKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);
            var certification = SymmetricEncryptionHandler.Decrypt(signaturePayload.Certification, certificationKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);

            if (!AsymmetricSignatureHandler.VerifySignature(signature.FromUtf8ToBase64(), asymmetricSignature, senderPublicKey))
                throw new PacketRejectedException(PacketRejectionReason.INVALID_RSA_SIGNATURE);

            return (signature, senderPublicKey, certification);
        }

        public (string senderIdentityProve, string message) ReceiveMessage(byte[] keyPacketBuffer, byte[] messagePacketBuffer, string receiverPrivateKey)
        {
            var keyPayload = Packet.Factory.FromBuffer<KeyPayload>(keyPacketBuffer).PeekPayload;
            var messagePayload = Packet.Factory.FromBuffer<MessagePayload>(messagePacketBuffer).PeekPayload;

            var messageKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.MessageKey, receiverPrivateKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_RSA_PRIVATE_KEY);
            var signatureKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.IdentityProveKey, receiverPrivateKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_RSA_PRIVATE_KEY);

            var message = SymmetricEncryptionHandler.Decrypt(messagePayload.MessageCipher, messageKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);
            var signature = SymmetricEncryptionHandler.Decrypt(messagePayload.IdentityProve, signatureKey) ?? throw new PacketRejectedException(PacketRejectionReason.INVALID_AES_KEY);

            return (signature, message);
        }
    }
}
