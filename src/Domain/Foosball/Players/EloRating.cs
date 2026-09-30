using Domain.Shared;

namespace Domain.Foosball.Players;

public sealed record EloRating : IValueObject
{
    public const int InitialPoints = 1000;

    private EloRating(int points) => Points = points;

    public static EloRating Initial { get; } = new(InitialPoints);

    public int Points { get; }

    // A rating can never go below zero: clamping keeps the invariant without forcing every caller to handle a failure.
    public static EloRating FromPoints(int points) => new(Math.Max(0, points));

    public override string ToString() => Points.ToString();
}
