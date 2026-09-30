using Application.Foosball.Players;
using Application.Shared.Events;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Microsoft.Extensions.Logging;

namespace Application.Foosball.Matches.EventHandlers;

internal sealed class UpdateRatingsOnMatchFinishedHandler(
    IPlayerRepository players,
    IFoosballUnitOfWork unitOfWork,
    ILogger<UpdateRatingsOnMatchFinishedHandler> logger) : IDomainEventHandler<MatchFinished>
{
    public async Task HandleAsync(MatchFinished domainEvent, CancellationToken cancellationToken = default)
    {
        Player? winner = await players.GetByIdAsync(domainEvent.WinnerId, cancellationToken);
        Player? loser = await players.GetByIdAsync(domainEvent.LoserId, cancellationToken);

        if (winner is null || loser is null)
        {
            logger.LogWarning("Ratings of match {MatchId} not updated: a player no longer exists", domainEvent.MatchId);

            return;
        }

        EloChange change = EloCalculator.Calculate(winner.Rating, loser.Rating);
        winner.UpdateRating(change.Winner);
        loser.UpdateRating(change.Loser);

        // The match is already saved when this handler runs and nothing saves on its behalf.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "{Winner} ({WinnerRating}) beat {Loser} ({LoserRating}) {RedScore}-{BlueScore}",
            winner.Name,
            winner.Rating,
            loser.Name,
            loser.Rating,
            domainEvent.RedScore,
            domainEvent.BlueScore);
    }
}
