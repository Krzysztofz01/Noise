using Noise.Core.Encryption;
using Noise.Core.Protocol;
using Noise.Core.Services;
using System.Collections.Generic;
using Xunit;

namespace Noise.Core.Test
{
    public class PacketServiceTests
    {
        [Fact]
        public void CreatePingPacketShouldCreatePacket()
        {
            var service = new PacketService();

            var packet = service.CreatePingPacket();

            var exptectedPacketType = PacketType.PING;

            Assert.Equal(exptectedPacketType, ((Packet)packet).Type);
        }

        [Fact]
        public void CreateDiscoveryAndRetriveDiscoveryPacketShouldExecute()
        {
            // The ,,first'' peer will send third peer discovery to second peer

            // Second peer RSA
            var secondAeh = new AsymmetricEncryptionHandler();
            string secondPublicKey = secondAeh.GetPublicKey();
            string secondPrivateKeyXml = secondAeh.GetPrivateKey();
            string secondIpEndpoint = "127.0.0.2";

            // Third peer RSA and IP
            var thirdAeh = new AsymmetricEncryptionHandler();
            string thirdPublicKey = thirdAeh.GetPublicKey();
            string thirdIpEndpoint = "127.0.0.3";

            var service = new PacketService();

            var publicKeys = new List<string>() { secondPublicKey, thirdPublicKey };
            var endpoints = new List<string>() { secondIpEndpoint, thirdIpEndpoint };

            var packetKeyDiscoveryPairs = service.CreateDiscoveryPackets(endpoints, publicKeys);

            // As the second peer try retrive packets
            foreach (var packetPair in packetKeyDiscoveryPairs)
            {
                var (retrivedPublicKeys, retrivedEndpoitns) = service.RetriveDiscoveryPacket(packetPair.keyPacket, packetPair.discoveryPacket, secondPrivateKeyXml);

                if (retrivedPublicKeys is not null && retrivedEndpoitns is not null)
                {
                    Assert.Equal(publicKeys, retrivedPublicKeys);
                    Assert.Equal(endpoints, retrivedEndpoitns);
                }
            }
        }

        [Fact]
        public void CreateMessageAndRetriveMessagePacketShouldExecute()
        {
            // First peer RSA and IP
            var firstAeh = new AsymmetricEncryptionHandler();
            string firstPublicKey = firstAeh.GetPublicKey();

            string message = "Hello World!";

            // Second peer RSA
            var secondAeh = new AsymmetricEncryptionHandler();
            string secondPublicKey = secondAeh.GetPublicKey();
            string secondPrivateKeyXml = secondAeh.GetPrivateKey();

            var service = new PacketService();
            var (keyPacket, messagePacket) = service.CreateMessagePackets(firstPublicKey, secondPublicKey, message);

            var (publicKey, messageResult) = service.RetriveMessagePacket(keyPacket, messagePacket, secondPrivateKeyXml);

            Assert.Equal(firstPublicKey, publicKey);
            Assert.Equal(message, messageResult);
        }
    }
}
