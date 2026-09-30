using Application.Shared.Messaging;

namespace Application.Foosball.Players.Leaderboard;

public sealed record LeaderboardQuery(int Top = 10) : IQuery<IReadOnlyList<LeaderboardEntry>>;

public sealed record LeaderboardEntry(int Rank, Guid PlayerId, string Name, int Rating);
