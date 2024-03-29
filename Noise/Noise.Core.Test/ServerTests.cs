﻿using Moq;
using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Core.Server;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Noise.Core.Test
{
    public class ServerTests
    {
        [Fact]
        public async void NoiseServerShouldStartWithoutConfiguration()
        {
            var outputMonitor = new Mock<IOutputMonitor>().Object;
            var peerConfiguration = MockupPeerConfiguration;

            using var server = new NoiseServer(outputMonitor, peerConfiguration);

            Assert.Throws<InvalidOperationException>(() => server.Stop());

            _ = Task.Run(async () => await server.StartAsync(CancellationToken.None));

            await Task.Delay(3000);

            _ = Assert.ThrowsAsync<InvalidOperationException>(async () => await server.StartAsync(CancellationToken.None));

            await Task.Delay(3000);
            
            server.Stop();
        }

        [Fact]
        public void ValidConfigurationShouldCreateNewInstance()
        {
            new NoiseServerConfiguration
            {
                VerboseMode = true,
                StreamBufferSize = 8000,
                EnableKeepAlive = false,
                KeepAliveInterval = 1,
                KeepAliveTime = 1,
                KeepAliveRetryCount = 1
            };
        }

        [Fact]
        public void InvalidConfigurationShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new NoiseServerConfiguration
                {
                    StreamBufferSize = 0,
                    KeepAliveInterval = 0,
                    KeepAliveTime = 0,
                    KeepAliveRetryCount = 0
                };
            });
        }

        private PeerConfiguration MockupPeerConfiguration =>
            PeerConfiguration.Factory.Initialize(Guid.NewGuid().ToString(), Constants.Version);
    }
}
