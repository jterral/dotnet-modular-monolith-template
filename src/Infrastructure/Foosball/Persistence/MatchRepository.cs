using Application.Foosball.Matches;
using Domain.Foosball.Matches;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Foosball.Persistence;

internal sealed class MatchRepository(FoosballDbContext context) : IMatchRepository
{
    private readonly FoosballDbContext _context = context;

    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Matches.FirstOrDefaultAsync(match => match.Id == id, cancellationToken);

    public async Task<bool> HasUnfinishedMatchAsync(Guid playerId, CancellationToken cancellationToken = default) =>
        await _context.Matches.AnyAsync(
            match => match.Status != MatchStatus.Finished
                && (match.RedPlayerId == playerId || match.BluePlayerId == playerId),
            cancellationToken);

    public async Task<IReadOnlyList<Match>> GetInProgressAsync(CancellationToken cancellationToken = default) =>
        await _context.Matches
            .AsNoTracking()
            .Where(match => match.Status == MatchStatus.InProgress)
            .ToListAsync(cancellationToken);

    public void Add(Match match) => _context.Matches.Add(match);
}
