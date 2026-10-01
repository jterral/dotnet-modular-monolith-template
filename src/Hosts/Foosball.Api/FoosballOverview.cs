using Application.Foosball.Matches;
using Application.Foosball.Players.Leaderboard;

namespace Foosball.Api;

/// <param name="Kind">"matches" when matches are in progress, "players" otherwise.</param>
public sealed record FoosballOverview(
    string Kind,
    IReadOnlyList<MatchResponse> Matches,
    IReadOnlyList<LeaderboardEntry> Players);
