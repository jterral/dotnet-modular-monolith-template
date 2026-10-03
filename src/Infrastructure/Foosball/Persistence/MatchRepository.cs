using Application.Foosball.Matches;
using Domain.Foosball.Matches;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Foosball.Persistence;

internal sealed class MatchRepository(FoosballDbContext context) : IMatchRepository
{
    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Matches.FirstOrDefaultAsync(match => match.Id == id, cancellationToken);
    }

    public async Task<bool> HasUnfinishedMatchAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await context.Matches.AnyAsync(
            match => match.Status != MatchStatus.Finished
                && (match.RedPlayerId == playerId || match.BluePlayerId == playerId),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetInProgressAsync(CancellationToken cancellationToken = default)
    {
        return await context.Matches
            .AsNoTracking()
            .Where(match => match.Status == MatchStatus.InProgress)
            .ToListAsync(cancellationToken);
    }

    public void Add(Match match)
    {
        context.Matches.Add(match);
    }
}
