using System.Collections.Generic;

namespace Noise.Core.Peer.Persistence
{
    public class PeerConfigurationPersistence
    {
        public IEnumerable<RemotePeerPersistence> RemotePeers { get; set; }
        public IEnumerable<PeerEndpointPersistence> PeerEndpoints { get; set; }
        public PeerSecretsPersistence Secrets { get; set; }
        public PeerPreferencesPersistence Preferences { get; set; }
    }
}
