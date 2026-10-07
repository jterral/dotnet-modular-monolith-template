using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;

namespace Application.Foosball.Matches.StartMatch;

internal sealed class StartMatchCommandHandler(
    IMatchRepository matches,
    IFoosballUnitOfWork unitOfWork) : ICommandHandler<StartMatchCommand>
{
    private readonly IMatchRepository _matches = matches;
    private readonly IFoosballUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(StartMatchCommand command, CancellationToken cancellationToken = default)
    {
        Match? match = await _matches.GetByIdAsync(command.MatchId, cancellationToken);

        if (match is null)
        {
            return Result.Failure(MatchErrors.NotFound);
        }

        Result started = match.Start();

        if (started.IsFailure)
        {
            return started;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
