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
    public class TcpSignatureIntegrationTests : IntegrationTestsUtility
    {
        [Fact]
        public async void ServerShouldreceiveClientsSignaturePacket()
        {
            var mockupOutputMonitor = MockupOutputMonitor();

            var mockupPeer1 = MockupPeerConfiguration();
            var mockupPeer2 = MockupPeerConfiguration();

            mockupPeer2.InsertPeer(mockupPeer1.PublicKey, SignatureBuilder.GenerateSignature());

            var received = false;
            void AssertEventOnSignatureReceived(object sender, SignatureReceivedEventArgs e) => received = true;

            using var server = new NoiseServer(mockupOutputMonitor, mockupPeer1);
            server.OnSignatureReceived += AssertEventOnSignatureReceived;
            _ = Task.Run(async () => await server.StartAsync(CancellationToken.None), CancellationToken.None);

            Thread.Sleep(Timeout);

            using var client = new NoiseClient(IPAddress.Loopback.ToString(), mockupOutputMonitor, mockupPeer2);

            await client.SendSignature(mockupPeer1.PublicKey);

            Thread.Sleep(Timeout);

            Assert.True(received);
        }
    }
}
