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
        public List<string> _peerEndpoints { get; set; }

        [Obsolete("Use the correct methods instead of the collection.")]
        public List<RemotePeer> _remotePeers { get; set; }

        public string PrivateKeyXml { get; init; }
        public string PublicKey { get; init; }
        public bool VerboseMode { get; init; }

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization and ,,private'' usage. Use one of the methods of the PeerConfiguration.Factory class.")]
        public PeerConfiguration()
        {
            _peerEndpoints = new List<string>();
            _remotePeers = new List<RemotePeer>();
        }

        private int GenerateRemotePeerIdentifier()
        {
            if (_remotePeers.Count == 0) return 0;

            return _remotePeers.Max(p => p.Identifier) + 1;
        }

        public IEnumerable<string> GetEndpoints()
        {
            return _peerEndpoints;
        }

        public void InsertEndpoints(IEnumerable<string> endpoints)
        {
            foreach (var endpoint in endpoints)
            {
                string ipAddress = endpoint.Split(':').First();

                if (!IPAddress.TryParse(ipAddress, out _))
                    throw new ArgumentException("Invalid endpoint format.", nameof(endpoints));

                if (_peerEndpoints.Any(e => e == ipAddress)) continue;

                _peerEndpoints.Add(ipAddress);
            }
        }

        public void InsertPeers(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (key.IsEmpty()) throw new ArgumentNullException(nameof(keys), "Invalid values.");

                if (_remotePeers.Any(p => p.PublicKey == key)) continue;

                _remotePeers.Add(RemotePeer.Factory.FromParameters(key, GenerateRemotePeerIdentifier()));
            }
        }

        public void InsertKey(string key, string alias = _defaultAlias)
        {
            if (key.IsEmpty() || alias.IsEmpty())
                throw new ArgumentNullException(nameof(key), "Invalid values.");

            if (_remotePeers.Any(p => p.PublicKey == key))
                throw new ArgumentNullException(nameof(key), "The key already exists.");

            if (_remotePeers.Any(p => p.Alias == alias && alias != _defaultAlias))
                throw new ArgumentNullException(nameof(key), "The alias must by unique.");

            _remotePeers.Add(RemotePeer.Factory.FromParameters(key, GenerateRemotePeerIdentifier(), alias));
        }

        public IEnumerable<RemotePeer> GetKeys()
        {
            return _remotePeers;
        }

        public void InsertAlias(string key, string alias)
        {
            if (key.IsEmpty() || alias.IsEmpty())
                throw new ArgumentNullException(nameof(key), "Invalid values.");

            if (!_remotePeers.Any(p => p.PublicKey == key))
                throw new ArgumentNullException(nameof(key), "The key does not exist.");

            if (_remotePeers.Any(p => p.Alias == alias))
                throw new ArgumentNullException(nameof(key), "The alias must by unique.");

            _remotePeers.Single(p => p.PublicKey == key).SetAlias(alias);
        }

        public RemotePeer GetPeerByAlias(string alias)
        {
            return _remotePeers.Single(p => p.Alias == alias);
        }

        public RemotePeer GetPeerByKey(string key)
        {
            return _remotePeers.Single(p => p.PublicKey == key);
        }

        public RemotePeer GetPeerByIdentifier(int id)
        {
            return _remotePeers.Single(p => p.Identifier == id);
        }

        public bool IsEndpointKnown(string endpoint)
        {
            return _peerEndpoints.Any(e => e == endpoint);
        }

        public bool IsPeerKnown(string publicKey)
        {
            return _remotePeers.Any(p => p.PublicKey == publicKey);
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

            public static PeerConfiguration Initialize()
            {
                var aeh = new AsymmetricEncryptionHandler();

                return new PeerConfiguration
                {
                    PrivateKeyXml = aeh.GetPrivateKey(),
                    PublicKey = aeh.GetPublicKey(),
                    VerboseMode = false,
                };
            }
        }
    }
    #pragma warning restore CS0618 // Type or member is obsolete
}
