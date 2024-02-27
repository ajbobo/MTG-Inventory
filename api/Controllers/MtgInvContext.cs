using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using mtg_api;

namespace mtg_api;

[ExcludeFromCodeCoverage]
public class MtgInvContext : DbContext
{
    public MtgInvContext(DbContextOptions<MtgInvContext> options)
        : base(options)
    {
    }

    public MtgInvContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultContainer("Misc");

        builder.Entity<CollectionInput>()
            .ToContainer("Collection")
            .HasPartitionKey("Key")
            .HasNoDiscriminator();

        builder.Entity<DeckData>()
            .ToContainer("Decks")
            .HasPartitionKey("Key")
            .HasNoDiscriminator();
    }

    public DbSet<CollectionInput> Collection { get; set; } = default!;
    public DbSet<DeckData> Decks { get; set; } = default!;
}