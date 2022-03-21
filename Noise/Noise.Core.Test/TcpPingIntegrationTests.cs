using Noise.Core.Client;
using Noise.Core.Server;
using Noise.Core.Server.Events;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Noise.Core.Test
{
    public class TcpPingIntegrationTests : IntegrationTestsUtility
    {
        [Fact]
        public async void ServerShouldReceiveClientsPingPacket()
        {
            var mockupOutputMonitor = MockupOutputMonitor();
            var mockupPeer = MockupPeerConfiguration();

            var received = false;
            void AssertEventOnPingReceived(object sender, PingReceivedEventArgs e) => received = true;

            using var server = new NoiseServer(mockupOutputMonitor, mockupPeer);
            server.OnPingReceived += AssertEventOnPingReceived;
            _ = Task.Run(async () => await server.StartAsync(CancellationToken.None), CancellationToken.None);

            Thread.Sleep(Timeout);

            using var client = new NoiseClient(IPAddress.Loopback.ToString(), mockupOutputMonitor, mockupPeer);
            await client.SendPing();

            Thread.Sleep(Timeout);

            Assert.True(received);
        }
    }
}
