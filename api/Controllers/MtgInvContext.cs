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

        builder.Entity<CollectionInput>()
            .ToContainer("Collection")
            .HasPartitionKey("Key")
            .HasNoDiscriminator();
    }

    public DbSet<CollectionInput> Collection { get; set; } = default!;
}