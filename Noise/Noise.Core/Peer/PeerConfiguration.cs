using Noise.Core.Encryption;
using Noise.Core.Exceptions;
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
        
        [Configurable]
        public bool UseTracker { get; set; }
        //[Configurable]
        public IEnumerable<string> Trackers { get; set; }
        [Configurable]
        public bool VerboseMode { get; set; }
        [Configurable]
        public string IndependentMediumCertification { get; set; }

        [JsonConstructor]
        [Obsolete("This constructor is only for deserialization and ,,private'' usage. Use one of the methods of the PeerConfiguration.Factory class.")]
        public PeerConfiguration()
        {
            Endpoints = new List<string>();
            Peers = new List<RemotePeer>();
            Trackers = new List<string>();
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

        public void InsertPeer(string publicKey, string receivingSignature = null, string alias = _defaultAlias)
        {
            if (publicKey.IsEmpty())
                throw new ArgumentNullException(nameof(publicKey), "Invalid public key for peer.");

            if (publicKey == PublicKey)
                throw new InvalidOperationException("Can not insert own public key.");

            if (alias.IsEmpty())
                throw new ArgumentNullException(nameof(alias), "Invalid alias value for peer.");

            if (Peers.Any(p => p.PublicKey == publicKey))
                throw new InvalidOperationException("Given public key already exists.");

            if (receivingSignature is not null && Peers.Any(p => p.ReceivingSignature == receivingSignature))
                throw new InvalidOperationException("Given signature already exists");

            if (Peers.Any(p => p.Alias == alias && alias != _defaultAlias))
                throw new InvalidOperationException("Given alias is alredy is usage.");

            Peers.Add(RemotePeer.Factory.FromParameters(publicKey, GenerateOrdinalNumberIdentifier(), receivingSignature, alias));
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
            return Peers.SingleOrDefault(p => p.Alias == alias) ??
                    throw new PeerDataException(PeerDataProblemType.ALIAS_NOT_FOUND);
        }

        public RemotePeer GetPeerByPublicKey(string publicKey)
        {
            return Peers.SingleOrDefault(p => p.PublicKey == publicKey) ??
                    throw new PeerDataException(PeerDataProblemType.PUBLIC_KEY_NOT_FOUND);
        }

        public RemotePeer GetPeerByOrdinalNumberIdentifier(int id)
        {
            return Peers.SingleOrDefault(p => p.Identifier == id) ??
                throw new PeerDataException(PeerDataProblemType.ORDINAL_NUMER_NOT_FOUND);
        }

        public RemotePeer GetPeerByReceivingSignature(string receivingSignature)
        {
            return Peers.SingleOrDefault(p => p.ReceivingSignature == receivingSignature) ??
                throw new PeerDataException(PeerDataProblemType.SIGNATURE_NOT_FOUND);
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

        public bool HasPeerAssignedSignature(string publicKey)
        {
            return Peers.Any(p => p.PublicKey == publicKey && p.ReceivingSignature is not null);
        }

        public bool IsReceivingSignatureValid(string receivingSignature)
        {
            return Peers.Any(p => p.ReceivingSignature == receivingSignature);
        }

        public IDictionary<string, string> GetConfiguration()
        {
            return typeof(PeerConfiguration)
                .GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurableAttribute)))
                .ToDictionary(k => k.Name, v => (v.GetValue(this, null) is null) ? "" : v.GetValue(this, null).ToString());
        }

        public bool ApplyConfiguration(string name, string value)
        {
            try
            {
                var property = typeof(PeerConfiguration)
                    .GetProperties()
                    .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurableAttribute)))
                    .Single(p => p.Name.ToLower() == name.ToLower());

                switch (property.PropertyType)
                {
                    case Type _ when property.PropertyType == typeof(bool):
                        property.SetValue(this, bool.Parse(value)); break;

                    case Type _ when property.PropertyType == typeof(string):
                        property.SetValue(this, value); break;

                    default:
                        throw new InvalidOperationException();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                    UseTracker = false,
                    Trackers = new List<string>(),
                    IndependentMediumCertification = null
                };
            }
        }
    }
    #pragma warning restore CS0618 // Type or member is obsolete
}
