using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;

namespace Application.Foosball.Matches.StartMatch;

internal sealed class StartMatchCommandHandler(
    IMatchRepository matches,
    IFoosballUnitOfWork unitOfWork) : ICommandHandler<StartMatchCommand>
{
    public async Task<Result> HandleAsync(StartMatchCommand command, CancellationToken cancellationToken = default)
    {
        Match? match = await matches.GetByIdAsync(command.MatchId, cancellationToken);

        if (match is null)
        {
            return Result.Failure(MatchErrors.NotFound);
        }

        Result started = match.Start();

        if (started.IsFailure)
        {
            return started;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
