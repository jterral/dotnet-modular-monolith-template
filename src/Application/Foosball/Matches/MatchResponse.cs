using Domain.Foosball.Matches;

namespace Application.Foosball.Matches;

public sealed record MatchResponse(
    Guid Id,
    Guid RedPlayerId,
    Guid BluePlayerId,
    int RedScore,
    int BlueScore,
    MatchStatus Status,
    Guid? WinnerId)
{
    internal static MatchResponse From(Match match) => new(
        match.Id,
        match.RedPlayerId,
        match.BluePlayerId,
        match.RedScore,
        match.BlueScore,
        match.Status,
        match.WinnerId);
}
