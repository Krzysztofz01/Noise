using Moq;
using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.Peer;
using System;
using Xunit;

namespace Noise.Core.Test
{
    public class ClientTests
    {
        [Fact]
        public void NoiseClientShouldCreateForValidEndpoint()
        {
            var outputMonitor = new Mock<IOutputMonitor>().Object;
            var peerConfiguration = MockupPeerConfiguration;

            var endpoint = "127.0.0.1";

            using var client = new NoiseClient(endpoint, outputMonitor, peerConfiguration);
        }

        [Fact]
        public void NoiseClientShouldThrowExceptionForInvalidEndpoint()
        {
            var outputMonitor = new Mock<IOutputMonitor>().Object;
            var peerConfiguration = MockupPeerConfiguration;

            var endpoint = "300.300.300.300";

            Assert.Throws<ArgumentException>(() => new NoiseClient(endpoint, outputMonitor, peerConfiguration));
        }

        [Fact]
        public void ValidConfigurationShouldCreateNewInstance()
        {
            new NoiseClientConfiguration
            {
                VerboseMode = true,
                StreamBufferSize = 8000,
                ConnectTimeoutMs = 2000,
                ReadTimeoutMs = 500
            };
        }

        [Fact]
        public void InvalidConfigurationShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new NoiseClientConfiguration
                {
                    StreamBufferSize = 0,
                    ConnectTimeoutMs = -1,
                    ReadTimeoutMs = -1
                };
            });
        }

        private PeerConfiguration MockupPeerConfiguration =>
            PeerConfiguration.Factory.Initialize(Guid.NewGuid().ToString());
    }
}
