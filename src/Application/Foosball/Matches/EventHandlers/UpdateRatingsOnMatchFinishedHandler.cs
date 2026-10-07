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
    private readonly IPlayerRepository _players = players;
    private readonly IFoosballUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UpdateRatingsOnMatchFinishedHandler> _logger = logger;

    public async Task HandleAsync(MatchFinished domainEvent, CancellationToken cancellationToken = default)
    {
        Player? winner = await _players.GetByIdAsync(domainEvent.WinnerId, cancellationToken);
        Player? loser = await _players.GetByIdAsync(domainEvent.LoserId, cancellationToken);

        if (winner is null || loser is null)
        {
            _logger.LogWarning("Ratings of match {MatchId} not updated: a player no longer exists", domainEvent.MatchId);

            return;
        }

        EloChange change = EloCalculator.Calculate(winner.Rating, loser.Rating);
        winner.UpdateRating(change.Winner);
        loser.UpdateRating(change.Loser);

        // The match is already saved when this handler runs and nothing saves on its behalf.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{Winner} ({WinnerRating}) beat {Loser} ({LoserRating}) {RedScore}-{BlueScore}",
            winner.Name,
            winner.Rating,
            loser.Name,
            loser.Rating,
            domainEvent.RedScore,
            domainEvent.BlueScore);
    }
}
