using Noise.Core.Peer.Persistence;
using System;
using System.Linq;
using System.Net;

namespace Noise.Core.Peer
{
    public class PeerEndpoint
    {
        public string Endpoint { get; private set; }
        public bool IsConnected { get; private set; }

        public void SetConnected() => IsConnected = true;
        public void SetDisconnected() => IsConnected = false;

        public PeerEndpointPersistence Serialize()
        {
            return new PeerEndpointPersistence
            {
                Endpoint = Endpoint
            };
        }

        private PeerEndpoint() { }
        public static class Factory
        {
            public static PeerEndpoint FromParameters(string endpoint)
            {
                string ipv4Address = endpoint.Split(':').First();

                if (!IPAddress.TryParse(ipv4Address, out _))
                    throw new ArgumentException("Invalid endpoint format.", nameof(endpoint));

                return new PeerEndpoint
                {
                    Endpoint = ipv4Address,
                    IsConnected = true
                };
            }

            public static PeerEndpoint Deserialize(PeerEndpointPersistence peerEndpoint)
            {
                return new PeerEndpoint
                {
                    Endpoint = peerEndpoint.Endpoint,
                    IsConnected = true
                };
            }
        }
    }
}
