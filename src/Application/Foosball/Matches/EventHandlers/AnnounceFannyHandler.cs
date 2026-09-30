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
    public async Task HandleAsync(FannyAchieved domainEvent, CancellationToken cancellationToken = default)
    {
        Player? winner = await players.GetByIdAsync(domainEvent.WinnerId, cancellationToken);
        Player? loser = await players.GetByIdAsync(domainEvent.LoserId, cancellationToken);

        logger.LogWarning(
            "FANNY! {Winner} wiped the floor with {Loser}: 10-0",
            winner?.Name.Value ?? domainEvent.WinnerId.ToString(),
            loser?.Name.Value ?? domainEvent.LoserId.ToString());
    }
}
