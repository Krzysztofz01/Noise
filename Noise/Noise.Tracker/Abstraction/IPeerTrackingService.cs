using Noise.Tracker.Models.Requests;
using Noise.Tracker.Models.Responses;
using System.Threading.Tasks;

namespace Noise.Tracker.Abstraction
{
    public interface IPeerTrackingService
    {
        Task InsertNoisePeer(InsertPeerRequest request);
        Task<RetrievePeerResponse> RetrieveNoisePeers();
    }
}
