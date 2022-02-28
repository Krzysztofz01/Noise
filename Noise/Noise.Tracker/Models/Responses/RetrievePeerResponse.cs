using Noise.Tracker.Dto;
using System.Collections.Generic;

namespace Noise.Tracker.Models.Responses
{
    public class RetrievePeerResponse
    {
        public IEnumerable<NoisePeerDto> Peers { get; set; }
    }
}
