using Microsoft.EntityFrameworkCore;
using Noise.Tracker.Abstraction;
using Noise.Tracker.Context;
using Noise.Tracker.Dto;
using Noise.Tracker.Entities;
using Noise.Tracker.Models.Requests;
using Noise.Tracker.Models.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noise.Tracker.Services
{
    public class PeerTrackingService : IPeerTrackingService
    {
        private readonly NoiseDbContext _noiseDbContext;

        public PeerTrackingService(NoiseDbContext noiseDbContext)
        {
            _noiseDbContext = noiseDbContext ??
                throw new ArgumentNullException(nameof(noiseDbContext));
        }

        public async Task InsertNoisePeer(InsertPeerRequest request)
        {
            var peer = new NoisePeer
            {
                PublicKey = request.PublicKey,
                Endpoint = request.Endpoint
            };

            _noiseDbContext.Add(peer);

            _ = await _noiseDbContext.SaveChangesAsync();
        }

        public async Task<RetrievePeerResponse> RetrieveNoisePeers()
        {
            var peers = await _noiseDbContext.NoisePeers
                .Select(p => new NoisePeerDto { PublicKey = p.PublicKey, Endpoint = p.Endpoint })
                .AsNoTracking()
                .ToListAsync();

            return new RetrievePeerResponse
            {
                Peers = peers
            };
        }
    }
}
