using Noise.Core.Abstraction;
using Noise.Core.Client;
using Noise.Core.Encryption;
using Noise.Core.Events;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using Noise.Core.Server;
using Noise.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Noise.Core.Test
{
    public class ClientServerTests
    {
        [Fact]
        public async void ServerShouldReceiveAndHandleClientPingPackets()
        {
            // Mockup server dependencies
            var mockupOutput = new MockupOutput();
            var mockupPacketService = new PacketService();
            var mockupPeerConfigurationServer = MockupPeerConfiguration();

            // Create server instance
            using var noiseServer = new NoiseServer(mockupOutput, mockupPacketService, mockupPeerConfigurationServer);
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedPacketType = PacketType.PING;
            var expectedEndpoint = IPAddress.Loopback.ToString();
            var expectedEndpointsList = new List<string> { "127.0.0.1" };
            var expectedKeysList = new List<string> { mockupPeerConfigurationServer.PublicKey };

            // Intercept event in order to assert
            noiseServer.OnPingReceived += AssertionPingEventHandler;
            void AssertionPingEventHandler(object sender, PacketReceivedEventsArgs e)
            {
                Assert.Equal(expectedPacketType, e.Packet.Type);
                Assert.Equal(expectedEndpoint, e.Client.Client.RemoteEndPoint.ToString());
            }

            // Create a new thread for sever to listen
            _ = Task.Run(async () => await noiseServer.StartAsync(ct));

            // Create new client instance and connect it to the server
            var mockupPeerConfigurationClient = MockupPeerConfiguration();
            mockupPeerConfigurationClient.InsertEndpoints(expectedEndpointsList);
            mockupPeerConfigurationClient.InsertPeers(expectedKeysList);
            using var noiseClient = new NoiseClient(mockupPeerConfigurationClient);

            // Prepare the packet and send it to the server via client
            var pingPacket = mockupPacketService.CreatePingPacket();
            await noiseClient.SendPacketAsync(pingPacket);
        }

        [Fact]
        public async void ServerShouldReceiveAndHandleClientDiscoveryPackets()
        {
            // Mockup server dependencies
            var mockupOutput = new MockupOutput();
            var mockupPacketService = new PacketService();
            var mockupPeerConfigurationServer = MockupPeerConfiguration();

            // Create server instance
            using var noiseServer = new NoiseServer(mockupOutput, mockupPacketService, mockupPeerConfigurationServer);
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedMessage = "New peers discovered!";
            var expectedEndpointsList = new List<string> { "127.0.0.1" };
            var expectedKeysList = new List<string> { mockupPeerConfigurationServer.PublicKey };

            // Create a new thread for sever to listen
            _ = Task.Run(async () => await noiseServer.StartAsync(ct));

            // Create new client instance and connect it to the server
            var mockupPeerConfigurationClient = MockupPeerConfiguration();
            mockupPeerConfigurationClient.InsertEndpoints(expectedEndpointsList);
            mockupPeerConfigurationClient.InsertPeers(expectedKeysList);
            using var noiseClient = new NoiseClient(mockupPeerConfigurationClient);

            // Prepare the packet and send it to the server via client
            var discovery = mockupPacketService.CreateDiscoveryPackets(expectedEndpointsList, expectedKeysList);
            foreach (var (keyPacket, discoveryPacket) in discovery)
            {
                await noiseClient.SendPacketsAsync(new List<IPacket> { keyPacket, discoveryPacket });
            }

            Thread.Sleep(4000);        
            Assert.Equal(expectedMessage, mockupOutput.Outputs.Single());
        }

        [Fact]
        public async void ServerShouldReceiveAndHandleClientMessagePackets()
        {
            // Mockup server dependencies
            var mockupOutput = new MockupOutput();
            var mockupPacketService = new PacketService();
            var mockupPeerConfigurationServer = MockupPeerConfiguration();

            // Create server instance
            using var noiseServer = new NoiseServer(mockupOutput, mockupPacketService, mockupPeerConfigurationServer);
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedMessage = "Hello World!";
            var expectedEndpointsList = new List<string> { "127.0.0.1" };
            var expectedKeysList = new List<string> { mockupPeerConfigurationServer.PublicKey };

            // Create a new thread for sever to listen
            _ = Task.Run(async () => await noiseServer.StartAsync(ct));

            // Create new client instance and connect it to the server
            var mockupPeerConfigurationClient = MockupPeerConfiguration();
            mockupPeerConfigurationClient.InsertEndpoints(expectedEndpointsList);
            mockupPeerConfigurationClient.InsertPeers(expectedKeysList);
            using var noiseClient = new NoiseClient(mockupPeerConfigurationClient);

            // Prepare the packet and send it to the server via client
            var messagePackets = mockupPacketService.CreateMessagePackets(mockupPeerConfigurationClient.PublicKey, mockupPeerConfigurationServer.PublicKey, expectedMessage);
            await noiseClient.SendPacketsAsync(new List<IPacket> { messagePackets.keyPacket, messagePackets.messagePacket });

            Thread.Sleep(4000);
            Assert.Equal(expectedMessage, mockupOutput.Outputs.Single());
        }

        public PeerConfiguration MockupPeerConfiguration()
        {
            return PeerConfiguration.Factory.Initialize();
        }
    }

    internal class MockupOutput : IOutput
    {
        public List<string> Outputs { get; set; }

        public MockupOutput() => Outputs = new List<string>();

        public void WriteException(string message, Exception ex)
        {
            Outputs.Add(message);
        }

        public void WriteLog(string message)
        {
            Outputs.Add(message);
        }

        public void WriteMessage(string senderPublicKey, string message, string senderIpAddress, string senderAlias)
        {
            Outputs.Add(message);
        }

        public void WritePing(string senderIpAddress)
        {
        }

        public void WriteRaw(string value)
        {
        }
    }
}
