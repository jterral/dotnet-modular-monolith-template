using Domain.Foosball.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Foosball.Persistence;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(player => player.Id);

        // Stored values were validated when the value object was created, so restoring them cannot fail.
        builder.Property(player => player.Name)
            .HasConversion(name => name.Value, value => PlayerName.Create(value).Value)
            .HasMaxLength(PlayerName.MaxLength)
            .IsRequired();

        builder.Property(player => player.Rating)
            .HasConversion(rating => rating.Points, points => EloRating.FromPoints(points));

        builder.HasIndex(player => player.Name).IsUnique();
    }
}
