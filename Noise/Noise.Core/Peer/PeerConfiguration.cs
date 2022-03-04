using Noise.Core.Encryption;
using Noise.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noise.Core.Peer
{
    #pragma warning disable CS0618 // Type or member is obsolete
    public class PeerConfiguration
    {
        private const string _defaultAlias = "Anonymous";

        [Obsolete("Use the correct methods instead of the collection.")]
        public List<string> Endpoints { get; set; }

        [Obsolete("Use the correct methods instead of the collection.")]
        public List<RemotePeer> Peers { get; set; }

        public string PrivateKey { get; init; }
        public string PublicKey { get; init; }
        public string ConfigurationSecret { get; init; }
        public bool VerboseMode { get; init; }
        public bool UseTracker { get; init; }

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization and ,,private'' usage. Use one of the methods of the PeerConfiguration.Factory class.")]
        public PeerConfiguration()
        {
            Endpoints = new List<string>();
            Peers = new List<RemotePeer>();
        }

        private int GenerateOrdinalNumberIdentifier()
        {
            if (Peers.Count == 0) return 0;

            return Peers.Max(p => p.Identifier) + 1;
        }

        public IEnumerable<string> GetEndpoints()
        {
            return Endpoints;
        }

        public IEnumerable<RemotePeer> GetPeers()
        {
            return Peers;
        }

        public void InsertEndpoint(string endpoint)
        {
            string ipv4Address = endpoint.Split(':').First();

            if (!IPAddress.TryParse(ipv4Address, out _))
                throw new ArgumentException("Invalid endpoint format.", nameof(endpoint));

            if (!Endpoints.Contains(ipv4Address))
                Endpoints.Add(ipv4Address);
        }

        public void InsertPeer(string publicKey, string signature, string alias = _defaultAlias)
        {
            if (publicKey.IsEmpty())
                throw new ArgumentNullException(nameof(publicKey), "Invalid public key for peer.");

            if (signature.IsEmpty())
                throw new ArgumentNullException(nameof(signature), "Invalid signature for peer.");

            if (alias.IsEmpty())
                throw new ArgumentNullException(nameof(alias), "Invalid alias value for peer.");

            if (Peers.Any(p => p.PublicKey == publicKey))
                throw new ArgumentNullException(nameof(publicKey), "Given public key is already exists.");

            if (Peers.Any(p => p.Alias == alias && alias != _defaultAlias))
                throw new ArgumentNullException(nameof(alias), "Given alias is alredy is usage.");

            Peers.Add(RemotePeer.Factory.FromParameters(publicKey, GenerateOrdinalNumberIdentifier(), signature, alias));
        }

        public void InsertAlias(string publicKey, string alias)
        {
            if (publicKey.IsEmpty())
                throw new ArgumentNullException(nameof(publicKey), "Invalid public key.");

            if (alias.IsEmpty())
                throw new ArgumentNullException(nameof(alias), "Invalid alias value for peer.");

            if (!Peers.Any(p => p.PublicKey == publicKey))
                throw new ArgumentNullException(nameof(publicKey), "The key does not exist.");

            if (Peers.Any(p => p.Alias == alias))
                throw new ArgumentNullException(nameof(alias), "Given alias is alredy is usage.");

            Peers.Single(p => p.PublicKey == publicKey).SetAlias(alias);
        }

        public RemotePeer GetPeerByAlias(string alias)
        {
            return Peers.Single(p => p.Alias == alias);
        }

        public RemotePeer GetPeerByPublicKey(string publicKey)
        {
            return Peers.Single(p => p.PublicKey == publicKey);
        }

        public RemotePeer GetPeerByOrdinalNumberIdentifier(int id)
        {
            return Peers.Single(p => p.Identifier == id);
        }

        public bool IsEndpointKnown(string endpoint)
        {
            string ipv4Address = endpoint.Split(':').First();
            return Endpoints.Any(e => e == ipv4Address);
        }

        public bool IsPeerKnown(string publicKey)
        {
            return Peers.Any(p => p.PublicKey == publicKey);
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                IncludeFields = false,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true
            });
        }

        public static class Factory
        {
            public static PeerConfiguration Deserialize(string serializedPeerConfiguration)
            {
                return JsonSerializer.Deserialize<PeerConfiguration>(serializedPeerConfiguration, new JsonSerializerOptions
                {
                    IncludeFields = false,
                    IgnoreReadOnlyFields = true,
                    IgnoreReadOnlyProperties = true
                });
            }

            public static PeerConfiguration Initialize(string configurationSecret)
            {
                if (configurationSecret.IsEmpty())
                    throw new ArgumentException("Invalid configuration secret.", nameof(configurationSecret));

                var privateKey = AsymmetricEncryptionHandler.InitializePrivateKey();
                var publicKey = AsymmetricEncryptionHandler.GetPublicKeyBase64(privateKey);

                return new PeerConfiguration
                {
                    PrivateKey = privateKey,
                    PublicKey = publicKey,
                    ConfigurationSecret = configurationSecret,
                    VerboseMode = false,
                    UseTracker = false
                };
            }
        }
    }
    #pragma warning restore CS0618 // Type or member is obsolete
}
