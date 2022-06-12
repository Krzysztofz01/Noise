using Noise.Core.Exceptions;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Noise.Core.Test
{
    public class PeerConfigurationTests
    {
        [Fact]
        public void PeerConfigurationShouldSerializeAndDeserialize()
        {
            var pc = MockUpPeerConfiguration();

            var serializedPc = pc.Serialize();

            var deserializedPc = PeerConfiguration.Factory.Deserialize(serializedPc);

            Assert.Equal(pc.Secrets.PrivateKey, deserializedPc.Secrets.PrivateKey);
        }

        [Fact]
        public void PeerShouldAddAndGetEndpointsDistinct()
        {
            var pc = MockUpPeerConfiguration();

            var endpoints = new List<string>
            {
                "127.0.0.1",
                "127.0.0.2",
                "127.0.0.1"
            };

            foreach (var endpoint in endpoints)
            {
                pc.InsertEndpoint(endpoint);
            }

            var expectedCount = 2;

            Assert.Equal(expectedCount, pc.GetEndpoints(false).Count());
        }

        [Fact]
        public void PeerShouldRemoveExistingEndpoint()
        {
            var pc = MockUpPeerConfiguration();

            var targetEndpoint = "127.0.0.2";

            Assert.Empty(pc.GetEndpoints());

            pc.InsertEndpoint(targetEndpoint);

            Assert.NotEmpty(pc.GetEndpoints());

            pc.RemoveEndpoint(targetEndpoint);

            Assert.Empty(pc.GetEndpoints());
        }

        [Fact]
        public void PeerShouldThrowOnRemovingNotExistingEndpoint()
        {
            var pc = MockUpPeerConfiguration();

            var targetEndpoint = "127.0.0.2";

            Assert.Empty(pc.GetEndpoints());

            Assert.Throws<PeerDataException>(() =>
            {
                pc.RemoveEndpoint(targetEndpoint);
            });
        }

        [Fact]
        public void PeerShouldThrowOnInvalidEndpoint()
        {
            var pc = MockUpPeerConfiguration();

            var invalidEndpoint = "Hello World!";

            Assert.Throws<ArgumentException>(() =>
            {
                pc.InsertEndpoint(invalidEndpoint);
            });
        }

        [Fact]
        public void PeerShouldAddAndGetPublicKeysDistinct()
        {
            var pc = MockUpPeerConfiguration();

            foreach (var _ in Enumerable.Range(0, 2))
            {
                pc.InsertPeer(
                    MockUpPeerConfiguration().Secrets.PublicKey,
                    MockUpPeerSignature());
            }

            var expectedCount = 2;

            Assert.Equal(expectedCount, pc.GetPeers().Count());
        }

        [Fact]
        public void PeerShouldRemoveExistingPublicKey()
        {
            var pc = MockUpPeerConfiguration();

            var targetPublicKey = MockUpPeerConfiguration().Secrets.PublicKey;

            Assert.Empty(pc.GetPeers());

            pc.InsertPeer(targetPublicKey, MockUpPeerSignature());

            Assert.NotEmpty(pc.GetPeers());

            pc.RemovePeer(targetPublicKey);

            Assert.Empty(pc.GetPeers());
        }

        [Fact]
        public void PeerShouldThrowOnRemovingNotExistingPublicKey()
        {
            var pc = MockUpPeerConfiguration();

            var targetPublicKey = MockUpPeerConfiguration().Secrets.PublicKey;

            Assert.Empty(pc.GetPeers());

            Assert.Throws<PeerDataException>(() =>
            {
                pc.RemovePeer(targetPublicKey);
            });
        }

        [Fact]
        public void PeerShouldAddAndGetPublicKeyWithAlias()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().Secrets.PublicKey;
            var signature = MockUpPeerSignature();
            var alias = "Hello World!";

            pc.InsertPeer(key, signature, alias);

            Assert.Equal(key, pc.GetPeers().Single().PublicKey);

            Assert.Equal(key, pc.GetPeerByAlias(alias).PublicKey);
            Assert.Equal(alias, pc.GetPeerByPublicKey(key).Alias);
        }

        [Fact]
        public void PeerShouldAddAliasToPeerWhenKeyExists()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().Secrets.PublicKey;
            var signature = MockUpPeerSignature();
            var alias = "Hello World!";

            pc.InsertPeer(key, signature);

            Assert.NotEqual(alias, pc.GetPeerByPublicKey(key).Alias);
            Assert.Throws<PeerDataException>(() =>
            {
                pc.GetPeerByAlias(alias);
            });

            pc.InsertAlias(key, alias);

            Assert.Equal(key, pc.GetPeerByAlias(alias).PublicKey);
            Assert.Equal(alias, pc.GetPeerByPublicKey(key).Alias);
        }

        [Fact]
        public void PeerShouldAddSendingSignatureToPeer()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().Secrets.PublicKey;
            var signature = MockUpPeerSignature();

            pc.InsertPeer(key, signature);

            var sendingSignature = MockUpPeerSignature();
            pc.GetPeerByPublicKey(key).SetSendingSignature(sendingSignature);

            var actualSendingSignature = pc.GetPeerByPublicKey(key).SendingSignature;

            Assert.Equal(sendingSignature, actualSendingSignature);
        }

        [Fact]
        public void PeerShouldThrowWhenAddingAliasToNotExistingKey()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().Secrets.PublicKey;
            var alias = "Hello World!";

            Assert.Throws<ArgumentNullException>(() =>
            {
                pc.InsertAlias(key, alias);
            });
        }

        [Fact]
        public void PeerShouldTellIfEndpointIsKnown()
        {
            var pc = MockUpPeerConfiguration();

            var knownEndpoint = "127.0.0.1";
            var unknownEndpoint = "127.0.0.2";

            pc.InsertEndpoint(knownEndpoint);

            Assert.True(pc.IsEndpointKnown(knownEndpoint));
            Assert.False(pc.IsEndpointKnown(unknownEndpoint));
        }

        [Fact]
        public void PeerShouldTellIfPeerIsKnown()
        {
            var pc = MockUpPeerConfiguration();

            var knownPeer = MockUpPeerConfiguration();
            var unknownPeer = MockUpPeerConfiguration();

            pc.InsertPeer(knownPeer.Secrets.PublicKey, MockUpPeerSignature());

            Assert.True(pc.IsPeerKnown(knownPeer.Secrets.PublicKey));
            Assert.False(pc.IsPeerKnown(unknownPeer.Secrets.PublicKey));
        }

        [Fact]
        public void PeerShouldGetKnownPeerByOrdinalNumber()
        {
            var pc = MockUpPeerConfiguration();

            var firstKey = MockUpPeerConfiguration().Secrets.PublicKey;
            var firstSignature = MockUpPeerSignature();
            pc.InsertPeer(firstKey, firstSignature);

            var secondKey = MockUpPeerConfiguration().Secrets.PublicKey;
            var secondSignature = MockUpPeerSignature();
            pc.InsertPeer(secondKey, secondSignature);

            var actualFirstKey = pc.GetPeerByOrdinalNumberIdentifier(0).PublicKey;
            var actualSecondKey = pc.GetPeerByOrdinalNumberIdentifier(1).PublicKey;

            Assert.Equal(firstKey, actualFirstKey);
            Assert.Equal(secondKey, actualSecondKey);
        }

        [Fact]
        public void PeerShouldGetKnownPeerBySignature()
        {
            var pc = MockUpPeerConfiguration();

            var knownKey = MockUpPeerConfiguration().Secrets.PublicKey;
            var knownSignature = MockUpPeerSignature();
            pc.InsertPeer(knownKey, knownSignature);

            var actualSignature = pc.GetPeerByReceivingSignature(knownSignature).ReceivingSignature;

            Assert.Equal(knownSignature, actualSignature);
        }

        [Fact]
        public void PeerShouldTellIfGivenSignatureIsValid()
        {
            var pc = MockUpPeerConfiguration();

            var firstKey = MockUpPeerConfiguration().Secrets.PublicKey;
            var firstSignature = MockUpPeerSignature();
            pc.InsertPeer(firstKey, firstSignature);

            var secondSignature = MockUpPeerSignature();

            var isFirstKeyValid = pc.IsReceivingSignatureValid(firstSignature);
            var isSecondKeyValid = pc.IsReceivingSignatureValid(secondSignature);

            Assert.True(isFirstKeyValid);
            Assert.False(isSecondKeyValid);
        }

        [Fact]
        public void PeerShouldAssignAndRetriveSignatures()
        {
            var pc = MockUpPeerConfiguration();
            var publicKey = MockUpPeerConfiguration().Secrets.PublicKey;
            pc.InsertPeer(publicKey);

            Assert.False(pc.IsReceivingSignatureDefinedForPeer(publicKey));
            Assert.False(pc.IsSendingSignatureDefinedForPeer(publicKey));

            var expectedReceivingSignature = MockUpPeerSignature();
            var expectedSendingSignature = MockUpPeerSignature();

            pc.SetReceivingSignatureForPeer(publicKey, expectedReceivingSignature);
            pc.SetSendingSignatureForPeer(publicKey, expectedSendingSignature);

            Assert.True(pc.IsReceivingSignatureDefinedForPeer(publicKey));
            Assert.True(pc.IsSendingSignatureDefinedForPeer(publicKey));

            var actualReceivingSignature = pc.GetReceivingSignatureForPeer(publicKey);
            var actualSendingSignature = pc.GetSendingSignatureForPeer(publicKey);

            Assert.Equal(expectedReceivingSignature, actualReceivingSignature);
            Assert.Equal(expectedSendingSignature, actualSendingSignature);
        }

        [Fact]
        public void PeerShouldTellIfASignatureIsAssignedToRemotePeer()
        {
            var pc = MockUpPeerConfiguration();

            var firstKey = MockUpPeerConfiguration().Secrets.PublicKey;
            var firstSignature = MockUpPeerSignature();
            pc.InsertPeer(firstKey, firstSignature);

            var secondKey = MockUpPeerConfiguration().Secrets.PublicKey;
            pc.InsertPeer(secondKey);

            Assert.True(pc.HasPeerAssignedSignature(firstKey));
            Assert.False(pc.HasPeerAssignedSignature(secondKey));
        }

        [Fact]
        public void PeerConfigurationShouldEncryptAndDecryptWithCorrectSecret()
        {
            var secret = "Hello World!";
            var peer = PeerConfiguration.Factory.Initialize(secret, Constants.Version);

            var encryptedPeerConfiguration = PeerEncryption.EncryptPeerConfiguration(peer);

            var decryptedPeerConfiguration = PeerEncryption.DecryptPeerConfiguration(encryptedPeerConfiguration, secret);

            Assert.NotNull(decryptedPeerConfiguration);
            Assert.Equal(peer.Secrets.PrivateKey, decryptedPeerConfiguration.Secrets.PrivateKey);
        }

        [Fact]
        public void PeerConfigurationShouldEncryptAndNotDecryptWithIncorrectSecret()
        {
            var secret = "Hello World!";
            var peer = PeerConfiguration.Factory.Initialize(secret, Constants.Version);

            var encryptedPeerConfiguration = PeerEncryption.EncryptPeerConfiguration(peer);

            var wrongSecret = "Hello World123!";
            var decryptedPeerConfiguration = PeerEncryption.DecryptPeerConfiguration(encryptedPeerConfiguration, wrongSecret);

            Assert.Null(decryptedPeerConfiguration);
        }

        [Fact]
        public void PeerShouldSkipDisconnectedEndpoints()
        {
            var pc = MockUpPeerConfiguration();

            var firstEndpoint = "127.0.0.1";
            pc.InsertEndpoint(firstEndpoint);

            var secondEndpoint = "127.0.0.2";
            pc.InsertEndpoint(secondEndpoint);

            pc.SetEndpointAsDisconnected(firstEndpoint);

            var expectedCount = 1;

            Assert.Equal(expectedCount, pc.GetEndpoints(true).Count());
        }

        [Fact]
        public void PeerShoulUpdateConnectedStatusForEndpoint()
        {
            var pc = MockUpPeerConfiguration();

            var firstEndpoint = "127.0.0.1";
            pc.InsertEndpoint(firstEndpoint);

            var secondEndpoint = "127.0.0.2";
            pc.InsertEndpoint(secondEndpoint);

            pc.SetEndpointAsDisconnected(firstEndpoint);

            var expectedCountBefore = 1;

            Assert.Equal(expectedCountBefore, pc.GetEndpoints(true).Count());

            pc.SetEndpointAsConnected(firstEndpoint);

            var expectedCountAfter = 2;

            Assert.Equal(expectedCountAfter, pc.GetEndpoints(true).Count());
        }

        [Fact]
        public void PeerShouldApplyPreference()
        {
            var pc = MockUpPeerConfiguration();

            var preferenceName = "verbosemode";
            var preferenceValue = "true";

            Assert.False(pc.Preferences.VerboseMode);

            Assert.True(pc.ApplyPreference(preferenceName, preferenceValue));

            Assert.True(pc.Preferences.VerboseMode);
        }

        [Fact]
        public void PeerShouldHandleWhenApplyingInvalidPreference()
        {
            var pc = MockUpPeerConfiguration();

            var preferenceName = "invalid_preference";
            var preferenceValue = "12true_helloworld";

            Assert.False(pc.ApplyPreference(preferenceName, preferenceValue));
        }

        public PeerConfiguration MockUpPeerConfiguration()
        {
            return PeerConfiguration.Factory.Initialize("Hello World!", Constants.Version);
        }

        public string MockUpPeerSignature()
        {
            return SignatureBuilder.GenerateSignature();
        }
    }
}
