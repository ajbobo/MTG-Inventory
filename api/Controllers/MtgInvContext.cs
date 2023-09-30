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
            .HasPartitionKey("Key")
            .HasNoDiscriminator();
    }

    public DbSet<CollectionEntry> Collection { get; set; } = default!;
}