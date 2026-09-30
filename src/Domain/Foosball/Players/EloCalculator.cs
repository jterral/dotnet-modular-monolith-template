namespace Domain.Foosball.Players;

public readonly record struct EloChange(EloRating Winner, EloRating Loser);

public static class EloCalculator
{
    // A high K-factor makes ratings move fast, which suits a handful of colleagues playing a few games.
    private const int KFactor = 32;

    public static EloChange Calculate(EloRating winner, EloRating loser)
    {
        double expectedWin = 1 / (1 + Math.Pow(10, (loser.Points - winner.Points) / 400d));
        int points = (int)Math.Round(KFactor * (1 - expectedWin), MidpointRounding.AwayFromZero);

        return new EloChange(
            EloRating.FromPoints(winner.Points + points),
            EloRating.FromPoints(loser.Points - points));
    }
}
