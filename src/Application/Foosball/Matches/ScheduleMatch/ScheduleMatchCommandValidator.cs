using Application.Shared;
using Domain.Shared;

namespace Application.Foosball.Matches.ScheduleMatch;

internal sealed class ScheduleMatchCommandValidator : IValidator<ScheduleMatchCommand>
{
    private static readonly Error PlayerIdRequired = new("Foosball.Match.PlayerIdRequired", "Both player ids are required.");

    public Task<Result> ValidateAsync(ScheduleMatchCommand instance, CancellationToken cancellationToken = default)
    {
        Result result = instance.RedPlayerId == Guid.Empty || instance.BluePlayerId == Guid.Empty
            ? Result.Failure(PlayerIdRequired)
            : Result.Success();

        return Task.FromResult(result);
    }
}
