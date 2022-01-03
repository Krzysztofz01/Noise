using Noise.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noise.Core.Models
{
    public class PeerConfiguration
    {
        [JsonIgnore]
        private const string _defaultAlias = "Anonymous";

        [JsonIgnore]
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            IncludeFields = false,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false
        };

        private readonly List<string> _peerEndpoints;
        private readonly HashSet<string> _peerKeys;
        private readonly Dictionary<string, string> _peerKeyAliasMap;

        public string PrivateKeyXml { get; private set; }
        public bool VerboseMode { get; private set; }

        private PeerConfiguration()
        {
            _peerEndpoints = new List<string>();
            _peerKeys = new HashSet<string>();
            _peerKeyAliasMap = new Dictionary<string, string>();
        }

        public IEnumerable<string> GetEndpoints()
        {
            return _peerEndpoints;
        }

        public void InsertEndpoint(string endpoint)
        {
            if (!IPAddress.TryParse(endpoint, out _))
                throw new ArgumentException("Invalid endpoint format.", nameof(endpoint));

            _peerEndpoints.Add(endpoint);
        }

        public void InsertKey(string key, string alias = _defaultAlias)
        {
            if (key.IsEmpty() || alias.IsEmpty())
                throw new ArgumentNullException("Invalid values.");

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

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, _serializerOptions);
        }

        public static class Factory
        {
            public static PeerConfiguration Deserialize(string serializedPeerConfiguration)
            {
                return JsonSerializer.Deserialize<PeerConfiguration>(serializedPeerConfiguration, _serializerOptions);
            }

            public static PeerConfiguration Initialize(string privateKeyXml)
            {
                return new PeerConfiguration
                {
                    PrivateKeyXml = privateKeyXml,
                    VerboseMode = true,
                };
            }
        }
    }
}
