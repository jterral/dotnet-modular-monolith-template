using Domain.Foosball.Matches;

namespace Application.Foosball.Matches;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasUnfinishedMatchAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Match>> GetInProgressAsync(CancellationToken cancellationToken = default);

    void Add(Match match);
}
