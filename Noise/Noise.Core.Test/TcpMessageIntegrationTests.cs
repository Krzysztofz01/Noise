using Noise.Core.Client;
using Noise.Core.Protocol;
using Noise.Core.Server;
using Noise.Core.Server.Events;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Noise.Core.Test
{
    public class TcpMessageIntegrationTests : IntegrationTestsUtility
    {
        [Fact]
        public async void ServerShouldReceiveClientsMessagePacket()
        {
            var mockupOutputMonitor = MockupOutputMonitor();

            var mockupPeer1 = MockupPeerConfiguration();
            var mockupPeer2 = MockupPeerConfiguration();

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

            Thread.Sleep(Timeout);

            using var client = new NoiseClient(IPAddress.Loopback.ToString(), mockupOutputMonitor, mockupPeer2);

            var message = "Hello World";
            await client.SendMessage(mockupPeer1.PublicKey, message);

            Thread.Sleep(Timeout);

            Assert.True(received);
        }
    }
}
