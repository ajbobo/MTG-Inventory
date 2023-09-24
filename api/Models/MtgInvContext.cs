using Microsoft.EntityFrameworkCore;
using mtg_api;

namespace mtg_api;

public class MtgInvContext : DbContext
{
    public MtgInvContext(DbContextOptions<MtgInvContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultContainer("Misc");

        builder.Entity<CollectionEntry>()
            .ToContainer("Collection")
            .HasPartitionKey("Uuid")
            .HasNoDiscriminator();
    }

    public DbSet<MTG_Card> Cards { get; set; } = default!;
    public DbSet<CollectionEntry> Collection { get; set; } = default!;
    // public DbSet<MTG_Set> Sets { get; set; } = default!;
}