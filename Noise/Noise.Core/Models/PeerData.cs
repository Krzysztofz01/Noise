using System.Collections.Generic;

namespace Noise.Core.Models
{
    public class PeerData
    {
        public IEnumerable<string> PublicKeys { get; set; }
        public IEnumerable<string> HostIpAddresses { get; set; }
    }
}
