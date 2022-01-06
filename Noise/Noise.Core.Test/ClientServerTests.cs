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
            var (mockupPeerConfigurationServer, publicKeyServer) = MockupPeerConfiguration();

            // Create server instance
            using var noiseServer = new NoiseServer(mockupOutput, mockupPacketService, mockupPeerConfigurationServer);
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedPacketType = PacketType.PING;
            var expectedEndpoint = IPAddress.Loopback.ToString();

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
            using var noiseClient = new NoiseClient();
            await noiseClient.ConnectAsync(IPAddress.Loopback.ToString());

            // Prepare the packet and send it to the server via client
            var pingPacket = mockupPacketService.CreatePingPacket();
            await noiseClient.SendPacketAsync(pingPacket);
            noiseClient.Disconnect();
        }

        [Fact]
        public async void ServerShouldReceiveAndHandleClientDiscoveryPackets()
        {
            // Mockup server dependencies
            var mockupOutput = new MockupOutput();
            var mockupPacketService = new PacketService();
            var (mockupPeerConfigurationServer, publicKeyServer) = MockupPeerConfiguration();

            // Create server instance
            using var noiseServer = new NoiseServer(mockupOutput, mockupPacketService, mockupPeerConfigurationServer);
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedMessage = "New peers discovered!";
            var expectedEndpointsList = new List<string> { "127.0.0.1" };
            var expectedKeysList = new List<string> { publicKeyServer };

            // Create a new thread for sever to listen
            _ = Task.Run(async () => await noiseServer.StartAsync(ct));

            // Create new client instance and connect it to the server
            var mockupPeerConfigurationClient = MockupPeerConfiguration();
            using var noiseClient = new NoiseClient();

            // Prepare the packet and send it to the server via client
            var discovery = mockupPacketService.CreateDiscoveryPackets(expectedEndpointsList, expectedKeysList);
            
            foreach(var endpoint in expectedEndpointsList)
            {
                await noiseClient.ConnectAsync(endpoint);

                foreach (var dsc in discovery)
                {
                    await noiseClient.SendPacketAsync(dsc.keyPacket);
                    await noiseClient.SendPacketAsync(dsc.discoveryPacket);
                }

                noiseClient.Disconnect();
            }

            Thread.Sleep(1500);
            
            Assert.Equal(expectedMessage, mockupOutput.Outputs.Single());
        }

        [Fact]
        public async void ServerShouldReceiveAndHandleClientMessagePackets()
        {
            // Mockup server dependencies
            var mockupOutput = new MockupOutput();
            var mockupPacketService = new PacketService();
            var (mockupPeerConfigurationServer, publicKeyServer) = MockupPeerConfiguration();

            // Create server instance
            using var noiseServer = new NoiseServer(mockupOutput, mockupPacketService, mockupPeerConfigurationServer);
            var ct = CancellationToken.None;

            // Prepare test data
            var expectedMessage = "Hello World!";
            var expectedEndpointsList = new List<string> { "127.0.0.1" };
            var expectedKeysList = new List<string> { publicKeyServer };

            // Create a new thread for sever to listen
            _ = Task.Run(async () => await noiseServer.StartAsync(ct));

            // Create new client instance and connect it to the server
            var (mockupPeerConfigurationClient, publicKeyClient) = MockupPeerConfiguration();
            using var noiseClient = new NoiseClient();

            // Prepare the packet and send it to the server via client
            var messagePackets = mockupPacketService.CreateMessagePackets(publicKeyClient, publicKeyServer, expectedMessage);

            foreach (var endpoint in expectedEndpointsList)
            {
                await noiseClient.ConnectAsync(endpoint);

                await noiseClient.SendPacketAsync(messagePackets.keyPacket);
                await noiseClient.SendPacketAsync(messagePackets.messagePacket);

                noiseClient.Disconnect();
            }

            Thread.Sleep(1500);

            Assert.Equal(expectedMessage, mockupOutput.Outputs.Single());
        }

        public (PeerConfiguration, string) MockupPeerConfiguration()
        {
            var aeh = new AsymmetricEncryptionHandler();

            return (PeerConfiguration.Factory.Initialize(aeh.GetPrivateKey()), aeh.GetPublicKey());
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
    }
}
