using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noise.Tracker.Entities;

namespace Noise.Tracker.Context.Builders
{
    public class NoisePeerTypeBuilder
    {
        public NoisePeerTypeBuilder(EntityTypeBuilder<NoisePeer> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.PublicKey).IsUnique();
            builder.HasIndex(e => e.Endpoint).IsUnique();
            builder.Property(e => e.FirstSeen).IsRequired();
        }
    }
}
