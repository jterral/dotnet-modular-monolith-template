using Application.Shared.Messaging;
using Domain.Foosball.Players;
using Domain.Shared;

namespace Application.Foosball.Players.Leaderboard;

internal sealed class LeaderboardQueryHandler(IPlayerRepository players)
    : IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>>
{
    private readonly IPlayerRepository _players = players;

    public async Task<Result<IReadOnlyList<LeaderboardEntry>>> HandleAsync(
        LeaderboardQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Player> top = await _players.GetTopByRatingAsync(Math.Max(0, query.Top), cancellationToken);

        IReadOnlyList<LeaderboardEntry> entries =
        [
            .. top.Select((player, index) => new LeaderboardEntry(index + 1, player.Id, player.Name.Value, player.Rating.Points))
        ];

        return Result.Success(entries);
    }
}
