using Application.Foosball.Matches;
using Application.Foosball.Players.Leaderboard;

namespace Foosball.Api.Endpoints;

/// <summary>
/// What is happening in the league right now: the matches in progress, or the players when none is.
/// </summary>
/// <param name="Kind">Which list is filled: <c>matches</c> when at least one match is in progress, <c>players</c> otherwise.</param>
/// <param name="Matches">Matches in progress. Empty when <paramref name="Kind"/> is <c>players</c>.</param>
/// <param name="Players">All players ranked by rating, best first. Empty when <paramref name="Kind"/> is <c>matches</c>.</param>
public sealed record FoosballOverview(
    string Kind,
    IReadOnlyList<MatchResponse> Matches,
    IReadOnlyList<LeaderboardEntry> Players);
