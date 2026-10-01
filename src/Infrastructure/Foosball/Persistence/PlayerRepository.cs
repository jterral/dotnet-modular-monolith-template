using Application.Foosball.Players;
using Domain.Foosball.Players;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Foosball.Persistence;

internal sealed class PlayerRepository(FoosballDbContext context) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Players.FirstOrDefaultAsync(player => player.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(PlayerName name, CancellationToken cancellationToken = default) =>
        await context.Players.AnyAsync(player => player.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Player>> GetTopByRatingAsync(int count, CancellationToken cancellationToken = default) =>
        await context.Players
            .AsNoTracking()
            .OrderByDescending(player => player.Rating)
            .ThenBy(player => player.Name)
            .Take(count)
            .ToListAsync(cancellationToken);

    public void Add(Player player) => context.Players.Add(player);
}
