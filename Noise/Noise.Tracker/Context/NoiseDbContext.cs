using Microsoft.EntityFrameworkCore;
using Noise.Tracker.Context.Builders;
using Noise.Tracker.Entities;

namespace Noise.Tracker.Context
{
    public class NoiseDbContext : DbContext
    {
        public NoiseDbContext(DbContextOptions<NoiseDbContext> options) : base(options) { }
    
        public virtual DbSet<NoisePeer> NoisePeers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = new NoisePeerTypeBuilder(modelBuilder.Entity<NoisePeer>());
        }
    }
}
