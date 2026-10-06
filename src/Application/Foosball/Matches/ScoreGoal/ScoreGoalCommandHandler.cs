using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;

namespace Application.Foosball.Matches.ScoreGoal;

internal sealed class ScoreGoalCommandHandler(
    IMatchRepository matches,
    IFoosballUnitOfWork unitOfWork) : ICommandHandler<ScoreGoalCommand>
{
    private readonly IMatchRepository _matches = matches;
    private readonly IFoosballUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(ScoreGoalCommand command, CancellationToken cancellationToken = default)
    {
        Match? match = await _matches.GetByIdAsync(command.MatchId, cancellationToken);

        if (match is null)
        {
            return Result.Failure(MatchErrors.NotFound);
        }

        Result scored = match.ScoreGoal(command.Side);

        if (scored.IsFailure)
        {
            return scored;
        }

        // Saving dispatches GoalScored and, on the last goal, MatchFinished (which updates the ratings).
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
