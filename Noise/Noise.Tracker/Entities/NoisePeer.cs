using System;

namespace Noise.Tracker.Entities
{
    public class NoisePeer
    {
        public NoisePeer()
        {
            Id = Guid.NewGuid();
            FirstSeen = DateTime.Now;
        }

        public Guid Id { get; set; }
        public string PublicKey { get; set; }
        public string Endpoint { get; set; }
        public DateTime FirstSeen { get; set; }
    }
}
