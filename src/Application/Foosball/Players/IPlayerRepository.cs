using Domain.Foosball.Players;

namespace Application.Foosball.Players;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(PlayerName name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Player>> GetTopByRatingAsync(int count, CancellationToken cancellationToken = default);

    void Add(Player player);
}
