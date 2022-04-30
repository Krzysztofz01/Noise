using Moq;
using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Core.Server;
using Noise.Core.Server.Events;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Noise.Core.Test
{
    [Collection("Sequential")]
    public class IntegrationTests
    {
        [Fact]
        public async void ServerShouldReceiveClientsMessagePacket()
        {
            var mockupOutputMonitor = MockupOutputMonitor;

            var mockupPeer1 = MockupPeerConfiguration;
            var mockupPeer2 = MockupPeerConfiguration;

            var peer1ToPeer2Signature = SignatureBuilder.GenerateSignature();
            var peer2ToPeer1Signature = SignatureBuilder.GenerateSignature();

            mockupPeer1.InsertPeer(mockupPeer2.PublicKey, peer2ToPeer1Signature);
            mockupPeer1.GetPeerByReceivingSignature(peer2ToPeer1Signature).SetSendingSignature(peer1ToPeer2Signature);

            mockupPeer2.InsertPeer(mockupPeer1.PublicKey, peer1ToPeer2Signature);
            mockupPeer2.GetPeerByReceivingSignature(peer1ToPeer2Signature).SetSendingSignature(peer2ToPeer1Signature);

            var received = false;
            void AssertEventOnMessageReceived(object sender, MessageReceivedEventArgs e) => received = true;

            using var server = new NoiseServer(mockupOutputMonitor, mockupPeer1);
            server.OnMessageReceived += AssertEventOnMessageReceived;
            _ = Task.Run(async () => await server.StartAsync(CancellationToken.None), CancellationToken.None);

            Thread.Sleep(MockupThrottleDelayMs);

            using var client = new NoiseClient(IPAddress.Loopback.ToString(), mockupOutputMonitor, mockupPeer2);

            var message = "Hello World";
            await client.SendMessage(mockupPeer1.PublicKey, message);

            Thread.Sleep(MockupThrottleDelayMs);

            Assert.True(received);
        }

        [Fact]
        public async void ServerShouldReceiveClientsPingPacket()
        {
            var mockupOutputMonitor = MockupOutputMonitor;
            var mockupPeer = MockupPeerConfiguration;

            var received = false;
            void AssertEventOnPingReceived(object sender, PingReceivedEventArgs e) => received = true;

            using var server = new NoiseServer(mockupOutputMonitor, mockupPeer);
            server.OnPingReceived += AssertEventOnPingReceived;
            _ = Task.Run(async () => await server.StartAsync(CancellationToken.None), CancellationToken.None);

            Thread.Sleep(MockupThrottleDelayMs);

            using var client = new NoiseClient(IPAddress.Loopback.ToString(), mockupOutputMonitor, mockupPeer);
            await client.SendPing();

            Thread.Sleep(MockupThrottleDelayMs);

            Assert.True(received);
        }

        [Fact]
        public async void ServerShouldreceiveClientsSignaturePacket()
        {
            var mockupOutputMonitor = MockupOutputMonitor;

            var mockupPeer1 = MockupPeerConfiguration;
            var mockupPeer2 = MockupPeerConfiguration;

            mockupPeer2.InsertPeer(mockupPeer1.PublicKey, SignatureBuilder.GenerateSignature());

            var received = false;
            void AssertEventOnSignatureReceived(object sender, SignatureReceivedEventArgs e) => received = true;

            using var server = new NoiseServer(mockupOutputMonitor, mockupPeer1);
            server.OnSignatureReceived += AssertEventOnSignatureReceived;
            _ = Task.Run(async () => await server.StartAsync(CancellationToken.None), CancellationToken.None);

            Thread.Sleep(MockupThrottleDelayMs);

            using var client = new NoiseClient(IPAddress.Loopback.ToString(), mockupOutputMonitor, mockupPeer2);
            await client.SendSignature(mockupPeer1.PublicKey);

            Thread.Sleep(MockupThrottleDelayMs);

            Assert.True(received);
        }

        private IOutputMonitor MockupOutputMonitor =>
            new Mock<IOutputMonitor>().Object;

        private PeerConfiguration MockupPeerConfiguration =>
            PeerConfiguration.Factory.Initialize(Guid.NewGuid().ToString());

        public static int MockupThrottleDelayMs => 10000;
    }
}
