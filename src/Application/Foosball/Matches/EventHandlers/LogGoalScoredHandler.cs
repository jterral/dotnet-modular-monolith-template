using Application.Shared.Events;
using Domain.Foosball.Matches;
using Microsoft.Extensions.Logging;

namespace Application.Foosball.Matches.EventHandlers;

internal sealed class LogGoalScoredHandler(ILogger<LogGoalScoredHandler> logger) : IDomainEventHandler<GoalScored>
{
    public Task HandleAsync(GoalScored domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GOAL for {Side}! Red {RedScore} - {BlueScore} Blue (match {MatchId})",
            domainEvent.Side,
            domainEvent.RedScore,
            domainEvent.BlueScore,
            domainEvent.MatchId);

        return Task.CompletedTask;
    }
}
