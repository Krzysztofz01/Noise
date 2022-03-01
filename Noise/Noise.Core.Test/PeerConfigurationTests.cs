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

            Assert.Equal(pc.PrivateKey, deserializedPc.PrivateKey);
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

            Assert.Equal(expectedCount, pc.GetEndpoints().Count());
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
                    MockUpPeerConfiguration().PublicKey,
                    MockUpPeerSignature());
            }

            var expectedCount = 2;

            Assert.Equal(expectedCount, pc.GetPeers().Count());
        }

        [Fact]
        public void PeerShouldAddAndGetPublicKeyWithAlias()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().PublicKey;
            var signature = MockUpPeerSignature();
            var alias = "Hello World!";

            pc.InsertPeer(key, signature, alias);

            Assert.Equal(key, pc.GetPeers().Single().PublicKey);

            Assert.Equal(key, pc.GetPeerByAlias(alias).PublicKey);
            Assert.Equal(alias, pc.GetPeerByPublicKey(key).Alias);
        }

        [Fact]
        public void PeerShouldAddAliasToKeyWhenKeyExists()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().PublicKey;
            var signature = MockUpPeerSignature();
            var alias = "Hello World!";

            pc.InsertPeer(key, signature);

            Assert.NotEqual(alias, pc.GetPeerByPublicKey(key).Alias);
            Assert.Throws<InvalidOperationException>(() =>
            {
                pc.GetPeerByAlias(alias);
            });

            pc.InsertAlias(key, alias);

            Assert.Equal(key, pc.GetPeerByAlias(alias).PublicKey);
            Assert.Equal(alias, pc.GetPeerByPublicKey(key).Alias);
        }

        [Fact]
        public void PeerShouldThrowWhenAddingAliasToNotExistingKey()
        {
            var pc = MockUpPeerConfiguration();

            var key = MockUpPeerConfiguration().PublicKey;
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

            pc.InsertPeer(knownPeer.PublicKey, MockUpPeerSignature());

            Assert.True(pc.IsPeerKnown(knownPeer.PublicKey));
            Assert.False(pc.IsPeerKnown(unknownPeer.PublicKey));
        }

        [Fact]
        public void PeerShouldGetKnownPeerByOrdinalNumber()
        {
            var pc = MockUpPeerConfiguration();

            var firstKey = MockUpPeerConfiguration().PublicKey;
            var firstSignature = MockUpPeerSignature();
            pc.InsertPeer(firstKey, firstSignature);

            var secondKey = MockUpPeerConfiguration().PublicKey;
            var secondSignature = MockUpPeerSignature();
            pc.InsertPeer(secondKey, secondSignature);

            var actualFirstKey = pc.GetPeerByOrdinalNumberIdentifier(0).PublicKey;
            var actualSecondKey = pc.GetPeerByOrdinalNumberIdentifier(1).PublicKey;

            Assert.Equal(firstKey, actualFirstKey);
            Assert.Equal(secondKey, actualSecondKey);
        }

        public PeerConfiguration MockUpPeerConfiguration()
        {
            return PeerConfiguration.Factory.Initialize();
        }

        public string MockUpPeerSignature()
        {
            return SignatureBuilder.GenerateSignature();
        }
    }
}
