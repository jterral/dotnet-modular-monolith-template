using Domain.Shared;

namespace Domain.Foosball.Players;

public sealed class Player : AggregateRoot
{
    // Required by EF Core to materialize the entity.
    private Player()
    {
    }

    // Always assigned by Create or by EF Core, never left null once the aggregate is usable.
    public PlayerName Name { get; private set; } = null!;

    public EloRating Rating { get; private set; } = EloRating.Initial;

    public static Player Create(PlayerName name)
    {
        return new() { Name = name };
    }

    public void UpdateRating(EloRating rating)
    {
        Rating = rating;
    }
}
