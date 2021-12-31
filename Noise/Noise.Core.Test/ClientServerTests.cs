using Noise.Core.Client;
using Noise.Core.Events;
using Noise.Core.Protocol;
using Noise.Core.Server;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Noise.Core.Test
{
    public class ClientServerTests
    {
        [Fact]
        public async void ServerShouldReceiveAndHandleClientPackets()
        {
            // Creating a new server instance and cancellation token
            using var noiseServer = new NoiseServer();
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedPacketType = PacketType.PING;
            var expectedPacketPayload = "Hello World!";

            // Create event handler method and asign it to the event
            noiseServer.OnPingReceived += packetReceiveEventHandler;
            void packetReceiveEventHandler(object sender, PacketReceivedEventsArgs e)
            {
                // Assert the incoming traffic
                Assert.Equal(expectedPacketType, e.Packet.Type);
                Assert.Equal(expectedPacketPayload, e.Packet.Payload);
            }

            // Set the server to listen on separate thread
            _ = Task.Run(async () => await noiseServer.StartAsync(ct));

            // Create new client instance and connect it to the server
            using var noiseClient = new NoiseClient();
            await noiseClient.ConnectAsync(IPAddress.Loopback.ToString());

            // Prepare the packet and send it to the server via client
            var packet = Packet.Factory.FromParameters(expectedPacketType, string.Empty);
            await noiseClient.SendPacketAsync(packet);          
        }
    }
}
