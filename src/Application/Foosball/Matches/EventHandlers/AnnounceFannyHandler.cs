using Application.Foosball.Players;
using Application.Shared.Events;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Microsoft.Extensions.Logging;

namespace Application.Foosball.Matches.EventHandlers;

internal sealed class AnnounceFannyHandler(
    IPlayerRepository players,
    ILogger<AnnounceFannyHandler> logger) : IDomainEventHandler<FannyAchieved>
{
    private readonly IPlayerRepository _players = players;
    private readonly ILogger<AnnounceFannyHandler> _logger = logger;

    public async Task HandleAsync(FannyAchieved domainEvent, CancellationToken cancellationToken = default)
    {
        Player? winner = await _players.GetByIdAsync(domainEvent.WinnerId, cancellationToken);
        Player? loser = await _players.GetByIdAsync(domainEvent.LoserId, cancellationToken);

        _logger.LogWarning(
            "FANNY! {Winner} wiped the floor with {Loser}: 10-0",
            winner?.Name.Value ?? domainEvent.WinnerId.ToString(),
            loser?.Name.Value ?? domainEvent.LoserId.ToString());
    }
}
