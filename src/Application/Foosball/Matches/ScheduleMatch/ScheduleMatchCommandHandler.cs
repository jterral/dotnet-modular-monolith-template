using Application.Foosball.Players;
using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Domain.Shared;

namespace Application.Foosball.Matches.ScheduleMatch;

internal sealed class ScheduleMatchCommandHandler(
    IPlayerRepository players,
    IMatchRepository matches,
    IFoosballUnitOfWork unitOfWork) : ICommandHandler<ScheduleMatchCommand, Guid>
{
    private readonly IPlayerRepository _players = players;
    private readonly IMatchRepository _matches = matches;
    private readonly IFoosballUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> HandleAsync(
        ScheduleMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<Match> scheduled = Match.Schedule(command.RedPlayerId, command.BluePlayerId);

        if (scheduled.IsFailure)
        {
            return Result.Failure<Guid>(scheduled.Error);
        }

        foreach (Guid playerId in new[] { command.RedPlayerId, command.BluePlayerId })
        {
            if (await _players.GetByIdAsync(playerId, cancellationToken) is null)
            {
                return Result.Failure<Guid>(PlayerErrors.NotFound);
            }

            if (await _matches.HasUnfinishedMatchAsync(playerId, cancellationToken))
            {
                return Result.Failure<Guid>(MatchErrors.PlayerBusy);
            }
        }

        _matches.Add(scheduled.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return scheduled.Value.Id;
    }
}
