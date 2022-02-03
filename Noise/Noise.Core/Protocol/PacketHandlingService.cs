using Noise.Core.Abstraction;
using Noise.Core.Encryption;
using Noise.Core.Exceptions;
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

        public (IPacket signaturePacket, string receiverIdentityProve) CreateSignaturePacket(string receiverPublicKey)
        {
            var signature = SignatureBuilder.GenerateSignature();

            var signatureCipher = AsymmetricEncryptionHandler.Encrypt(signature, receiverPublicKey);

            var signaturePayload = SignaturePayload.Factory.Create(signatureCipher);
            var signaturePacket = Packet<SignaturePayload>.Factory.FromPayload(signaturePayload);

            return (signaturePacket, signature);
        }

        public (IEnumerable<string> endpoints, IEnumerable<string> publicKeys, string senderIdentityProve) ReceiveDiscoveryCollections(byte[] keyPacketBuffer, byte[] discoveryPacketBuffer, string receiverPrivateKey)
        {
            var keyPayload = Packet.Factory.FromBuffer<KeyPayload>(keyPacketBuffer).PeekPayload;
            var discoveryPayload = Packet.Factory.FromBuffer<DiscoveryPayload>(discoveryPacketBuffer).PeekPayload;

            var discoveryKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.MessageKey, receiverPrivateKey) ?? throw new PacketRejectedException();
            var signatureKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.IdentityProveKey, receiverPrivateKey) ?? throw new PacketRejectedException();

            var endpointsKey = discoveryKey.Split(',').First();
            var publicKeysKey = discoveryKey.Split(',').Last();

            var signature = SymmetricEncryptionHandler.Decrypt(discoveryPayload.IdentityProve, signatureKey) ?? throw new PacketRejectedException();
            var endpoints = SymmetricEncryptionHandler.Decrypt(discoveryPayload.Endpoints, endpointsKey) ?? throw new PacketRejectedException();
            var publicKeys = SymmetricEncryptionHandler.Decrypt(discoveryPayload.PublicKeys, publicKeysKey) ?? throw new PacketRejectedException();

            var deserializedEndpoints = JsonSerializer.Deserialize<IEnumerable<string>>(endpoints);
            var deserializedPublicKeys = JsonSerializer.Deserialize<IEnumerable<string>>(publicKeys);

            return (deserializedEndpoints, deserializedPublicKeys, signature);
        }

        public string ReceiveIdentityProve(byte[] signaturePacketBuffer, string receiverPrivateKey)
        {
            var signaturePayload = Packet.Factory.FromBuffer<SignaturePayload>(signaturePacketBuffer).PeekPayload;

            return AsymmetricEncryptionHandler.Decrypt(signaturePayload.Signature, receiverPrivateKey) ?? throw new PacketRejectedException();
        }

        public (string senderIdentityProve, string message) ReceiveMessage(byte[] keyPacketBuffer, byte[] messagePacketBuffer, string receiverPrivateKey)
        {
            var keyPayload = Packet.Factory.FromBuffer<KeyPayload>(keyPacketBuffer).PeekPayload;
            var messagePayload = Packet.Factory.FromBuffer<MessagePayload>(messagePacketBuffer).PeekPayload;

            var messageKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.MessageKey, receiverPrivateKey) ?? throw new PacketRejectedException();
            var signatureKey = AsymmetricEncryptionHandler.Decrypt(keyPayload.IdentityProveKey, receiverPrivateKey) ?? throw new PacketRejectedException();

            var message = SymmetricEncryptionHandler.Decrypt(messagePayload.MessageCipher, messageKey) ?? throw new PacketRejectedException();
            var signature = SymmetricEncryptionHandler.Decrypt(messagePayload.IdentityProve, signatureKey) ?? throw new PacketRejectedException();

            return (signature, message);
        }
    }
}
