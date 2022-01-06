using Noise.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noise.Core.Peer
{
    public class PeerConfiguration
    {
        [JsonIgnore]
        private const string _defaultAlias = "Anonymous";

        private List<string> _peerEndpoints;
        private HashSet<string> _peerKeys;
        private Dictionary<string, string> _peerKeyAliasMap;

        public string PrivateKeyXml { get; init; }
        public bool VerboseMode { get; init; }

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization and ,,private'' usage. Use one of the methods of the PeerConfiguration.Factory class.")]
        public PeerConfiguration()
        {
            _peerEndpoints = new List<string>();
            _peerKeys = new HashSet<string>();
            _peerKeyAliasMap = new Dictionary<string, string>();
        }

        public IEnumerable<string> GetEndpoints()
        {
            return _peerEndpoints;
        }

        public void InsertEndpoints(IEnumerable<string> endpoints)
        {
            foreach (var endpoint in endpoints)
            {
                if (!IPAddress.TryParse(endpoint, out _))
                    throw new ArgumentException("Invalid endpoint format.", nameof(endpoints));
            }

            _peerEndpoints.AddRange(endpoints);

            _peerEndpoints = _peerEndpoints.Distinct().ToList();
        }

        public void InsertKeys(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                if (key.IsEmpty()) throw new ArgumentNullException(nameof(keys), "Invalid values.");

            _peerKeys.UnionWith(keys);

            _peerKeys = _peerKeys.Distinct().ToHashSet();
        }

        public void InsertKey(string key, string alias = _defaultAlias)
        {
            if (key.IsEmpty() || alias.IsEmpty())
                throw new ArgumentNullException(nameof(key), "Invalid values.");

            if (_peerKeys.Contains(key)) return;
            _peerKeys.Add(key);

            if (_peerKeyAliasMap.ContainsKey(key)) return;
            _peerKeyAliasMap.Add(key, alias);
        }

        public IEnumerable<string> GetKeys()
        {
            return _peerKeys;
        }

        public void InsertAlias(string key, string alias)
        {
            if (key.IsEmpty() || alias.IsEmpty())
                throw new ArgumentNullException("Invalid values.");

            if (!_peerKeys.Contains(key))
                throw new ArgumentNullException("Can not assign alias for unknown key.");

            if (_peerKeyAliasMap.ContainsKey(key))
            {
                _peerKeyAliasMap[key] = alias;
                return;
            }

            _peerKeyAliasMap.Add(key, alias);
        }

        public string GetKeyByAlias(string alias)
        {
            return _peerKeyAliasMap.Single(k => k.Value == alias).Key;
        }

        public string GetAliasByKey(string key)
        {
            if (_peerKeyAliasMap.ContainsKey(key))
                return _peerKeyAliasMap[key];

            return null;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                IncludeFields = true,
                IgnoreReadOnlyFields = false,
                IgnoreReadOnlyProperties = false
            });
        }

        public static class Factory
        {
            public static PeerConfiguration Deserialize(string serializedPeerConfiguration)
            {
                return JsonSerializer.Deserialize<PeerConfiguration>(serializedPeerConfiguration, new JsonSerializerOptions
                {
                    IncludeFields = true,
                    IgnoreReadOnlyFields = false,
                    IgnoreReadOnlyProperties = false
                });
            }

            public static PeerConfiguration Initialize(string privateKeyXml)
            {
                #pragma warning disable CS0618 // Type or member is obsolete
                return new PeerConfiguration
                {
                    PrivateKeyXml = privateKeyXml,
                    VerboseMode = false,
                };
                #pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
