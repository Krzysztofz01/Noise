using Noise.Core.Exceptions;
using Noise.Core.Extensions;
using Noise.Core.Peer.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Noise.Core.Peer
{
    public class PeerConfiguration
    {
        private const int _publicKeyLength = 684;

        private IList<PeerEndpoint> _peerEndpoints;
        private IList<RemotePeer> _remotePeers;

        public PeerSecrets Secrets { get; private set; }
        public PeerPreferences Preferences { get; private set; }

        public string Version { get; private set; }
        
        private int GenerateOrdinalNumberIdentifier()
        {
            if (!_remotePeers.Any()) return 0;

            return _remotePeers.Max(p => p.Identifier) + 1;
        }

        public IEnumerable<PeerEndpoint> GetEndpoints(bool onlyConnected = true)
        {
            if (!onlyConnected) return _peerEndpoints;

            return _peerEndpoints
                .Where(e =>
                    e.IsConnected ||
                    e.LastRequestAttempt is null ||
                    e.LastRequestAttempt.Value.AddSeconds(Preferences.EndpointAttemptIntervalSeconds) < DateTime.Now);
        }

        public IEnumerable<RemotePeer> GetPeers()
        {
            return _remotePeers;
        }

        public void InsertEndpoint(string endpoint)
        {
            var peerEndpoint = PeerEndpoint.Factory.FromParameters(endpoint);

            if (_peerEndpoints.Any(e => e.Endpoint == peerEndpoint.Endpoint)) return;

            _peerEndpoints.Add(peerEndpoint);
        }

        public void SetEndpointAsDisconnected(string endpoint)
        {
            if (!IsEndpointKnown(endpoint))
                throw new PeerDataException(PeerDataProblemType.ENDPOINT_NOT_FOUND);

            _peerEndpoints.Single(e => e.Endpoint == endpoint).SetDisconnected();
        }

        public void SetEndpointAsConnected(string endpoint)
        {
            if (!IsEndpointKnown(endpoint))
                throw new PeerDataException(PeerDataProblemType.ENDPOINT_NOT_FOUND);

            _peerEndpoints.Single(e => e.Endpoint == endpoint).SetConnected();
        }

        public void InsertPeer(string publicKey, string receivingSignature = null, string alias = null)
        {
            if (publicKey.IsEmpty())
                throw new ArgumentNullException(nameof(publicKey), "Invalid public key for peer.");

            if (publicKey.Length != _publicKeyLength && Preferences.FixedPublicKeyValidationLength)
                throw new InvalidOperationException("Invalid public key format or length.");

            if (publicKey == Secrets.PublicKey)
                throw new InvalidOperationException("Can not insert own public key.");
  
            if (_remotePeers.Any(p => p.PublicKey == publicKey))
                throw new InvalidOperationException("Given public key already exists.");

            if (receivingSignature is not null && _remotePeers.Any(p => p.ReceivingSignature == receivingSignature))
                throw new InvalidOperationException("Given signature already exists");

            if (_remotePeers.Any(p => p.Alias == alias))
                throw new InvalidOperationException("Given alias is alredy is usage.");

            _remotePeers.Add(RemotePeer.Factory.FromParameters(publicKey, GenerateOrdinalNumberIdentifier(), receivingSignature, alias));
        }

        public void InsertAlias(string publicKey, string alias)
        {
            if (publicKey.IsEmpty())
                throw new ArgumentNullException(nameof(publicKey), "Invalid public key.");

            if (!_remotePeers.Any(p => p.PublicKey == publicKey))
                throw new ArgumentNullException(nameof(publicKey), "The key does not exist.");

            if (_remotePeers.Any(p => p.Alias == alias))
                throw new ArgumentNullException(nameof(alias), "Given alias is alredy is usage.");

            _remotePeers.Single(p => p.PublicKey == publicKey).SetAlias(alias);
        }

        public RemotePeer GetPeerByAlias(string alias)
        {
            return _remotePeers.SingleOrDefault(p => p.Alias == alias) ??
                throw new PeerDataException(PeerDataProblemType.ALIAS_NOT_FOUND);
        }

        public RemotePeer GetPeerByPublicKey(string publicKey)
        {
            return _remotePeers.SingleOrDefault(p => p.PublicKey == publicKey) ??
                throw new PeerDataException(PeerDataProblemType.PUBLIC_KEY_NOT_FOUND);
        }

        public RemotePeer GetPeerByOrdinalNumberIdentifier(int id)
        {
            return _remotePeers.SingleOrDefault(p => p.Identifier == id) ??
                throw new PeerDataException(PeerDataProblemType.ORDINAL_NUMER_NOT_FOUND);
        }

        public RemotePeer GetPeerByReceivingSignature(string receivingSignature)
        {
            return _remotePeers.SingleOrDefault(p => p.ReceivingSignature == receivingSignature) ??
                throw new PeerDataException(PeerDataProblemType.SIGNATURE_NOT_FOUND);
        }

        public bool IsEndpointKnown(string endpoint)
        {
            string ipv4Address = endpoint.Split(':').First();
            return _peerEndpoints.Any(e => e.Endpoint == ipv4Address);
        }

        public bool IsPeerKnown(string publicKey)
        {
            return _remotePeers.Any(p => p.PublicKey == publicKey);
        }

        public bool HasPeerAssignedSignature(string publicKey)
        {
            return _remotePeers.Any(p => p.PublicKey == publicKey && p.ReceivingSignature is not null);
        }

        public bool IsReceivingSignatureValid(string receivingSignature)
        {
            return _remotePeers.Any(p => p.ReceivingSignature == receivingSignature);
        }

        public IDictionary<string, string> GetPreferences()
        {
            return Preferences.GetPreferences();
        }

        public bool ApplyPreference(string name, string value)
        {
            return Preferences.ApplyPreference(name, value);
        }

        public bool IsVersionValid(string hostVersion)
        {
            if (Preferences.AllowHostVersionMismatch) return true;

            try
            {
                var currentVersionArray = Version.Replace('.', '-').Skip(1).Where(x => x != '-');
                var currentMajor = Convert.ToInt32(currentVersionArray.First().ToString());
                var currentMinor = Convert.ToInt32(currentVersionArray.Skip(1).First().ToString());
                var currentPatch = Convert.ToInt32(currentVersionArray.Skip(2).First().ToString());

                var hostVersionArrray = hostVersion.Replace('.', '-').Skip(1).Where(x => x != '-');
                var hostMajor = Convert.ToInt32(hostVersionArrray.First().ToString());
                var hostMinor = Convert.ToInt32(hostVersionArrray.Skip(1).First().ToString());
                var hostPatch = Convert.ToInt32(hostVersionArrray.Skip(2).First().ToString());

                if (hostMajor < currentMajor) return false;
                if (hostMinor < currentMinor) return false;
                if (hostPatch < currentPatch) return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdatePeerVersion(string hostVersion)
        {
            if (!IsVersionValid(hostVersion))
                throw new PeerDataException(PeerDataProblemType.VERSION_MISMATCH);

            Version = hostVersion;
        }

        public string Serialize()
        {
            var persistenceConfigurationForm = new PeerConfigurationPersistence
            {
                PeerEndpoints = _peerEndpoints.Select(e => e.Serialize()),
                RemotePeers = _remotePeers.Select(e => e.Serialize()),
                Preferences = Preferences.Serialize(),
                Secrets = Secrets.Serialize(),
                Version = Version
            };

            return JsonSerializer.Serialize(persistenceConfigurationForm, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = false,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true
            });
        }


        private PeerConfiguration()
        {
            _peerEndpoints = new List<PeerEndpoint>();
            _remotePeers = new List<RemotePeer>();
        }

        public static class Factory
        {
            public static PeerConfiguration Deserialize(string serializedPeerConfiguration)
            {
                var persistenceConfigurationForm = JsonSerializer.Deserialize<PeerConfigurationPersistence>(serializedPeerConfiguration, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = false,
                    IgnoreReadOnlyFields = true,
                    IgnoreReadOnlyProperties = true
                });

                return new PeerConfiguration
                {
                    _peerEndpoints = persistenceConfigurationForm.PeerEndpoints.Select(e => PeerEndpoint.Factory.Deserialize(e)).ToList(),
                    _remotePeers = persistenceConfigurationForm.RemotePeers.Select(e => RemotePeer.Factory.Deserialize(e)).ToList(),
                    Secrets = PeerSecrets.Factory.Deserialize(persistenceConfigurationForm.Secrets),
                    Preferences = PeerPreferences.Factory.Deserialize(persistenceConfigurationForm.Preferences),
                    Version = persistenceConfigurationForm.Version
                };
            }

            public static PeerConfiguration Initialize(string configurationSecret, string version)
            {
                if (configurationSecret.IsEmpty())
                    throw new ArgumentException("Invalid configuration secret.", nameof(configurationSecret));

                if (version.IsEmpty())
                    throw new ArgumentException("Invalid host version.", nameof(version));

                var preferences = PeerPreferences.Factory.Initialize();
                var secrets = PeerSecrets.Factory.FromParameters(configurationSecret);

                return new PeerConfiguration
                {
                    Preferences = preferences,
                    Secrets = secrets,
                    Version = version
                };
            }
        }
    }
}
