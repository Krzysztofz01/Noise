using System.Collections.Generic;

namespace Noise.Core.Models
{
    public class DiscoveryDataModel
    {
        public IEnumerable<string> PublicKeys { get; set; }
        public IEnumerable<string> Endpoints { get; set; }
    }
}
