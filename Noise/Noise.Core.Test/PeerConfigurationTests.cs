using Noise.Core.Peer;
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

            Assert.Equal(pc.PrivateKeyXml, deserializedPc.PrivateKeyXml);
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

            pc.InsertEndpoints(endpoints);

            var expectedCount = 2;

            Assert.Equal(expectedCount, pc.GetEndpoints().Count());
        }

        [Fact]
        public void PeerShouldThrowOnInvalidEndpoint()
        {
            var pc = MockUpPeerConfiguration();

            var endpoints = new List<string>
            {
                "900.0.0.1",
                "Hello World!"
            };

            Assert.Throws<ArgumentException>(() =>
            {
                pc.InsertEndpoints(endpoints);
            });
        }

        [Fact]
        public void PeerShouldAddAndGetPublicKeysDistinct()
        {
            var pc = MockUpPeerConfiguration();

            var keys = new List<string>
            {
                "AAA",
                "BBB",
                "AAA"
            };

            pc.InsertPeers(keys);

            var expectedCount = 2;

            Assert.Equal(expectedCount, pc.GetKeys().Count());
        }

        [Fact]
        public void PeerShouldAddAndGetPublicKeyWithAlias()
        {
            var pc = MockUpPeerConfiguration();

            var key = "AAA";
            var alias = "Hello World!";

            pc.InsertKey(key, alias);

            Assert.Equal(key, pc.GetKeys().Single().PublicKey);

            Assert.Equal(key, pc.GetPeerByAlias(alias).PublicKey);
            Assert.Equal(alias, pc.GetPeerByKey(key).Alias);
        }

        [Fact]
        public void PeerShouldAddAliasToKeyWhenKeyExists()
        {
            var pc = MockUpPeerConfiguration();

            var key = "AAA";
            var alias = "Hello World!";

            pc.InsertKey(key);

            Assert.NotEqual(alias, pc.GetPeerByKey(key).Alias);
            Assert.Throws<InvalidOperationException>(() =>
            {
                pc.GetPeerByAlias(alias);
            });

            pc.InsertAlias(key, alias);

            Assert.Equal(key, pc.GetPeerByAlias(alias).PublicKey);
            Assert.Equal(alias, pc.GetPeerByKey(key).Alias);
        }

        [Fact]
        public void PeerShouldThrowWhenAddingAliasToNotExistingKey()
        {
            var pc = MockUpPeerConfiguration();

            var key = "AAA";
            var alias = "Hello World!";

            Assert.Throws<ArgumentNullException>(() =>
            {
                pc.InsertAlias(key, alias);
            });
        }

        public PeerConfiguration MockUpPeerConfiguration()
        {
            return PeerConfiguration.Factory.Initialize();
        }
    }
}
