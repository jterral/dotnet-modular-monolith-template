using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;

namespace Application.Foosball.Matches.ListInProgressMatches;

internal sealed class ListInProgressMatchesQueryHandler(IMatchRepository matches)
    : IQueryHandler<ListInProgressMatchesQuery, IReadOnlyList<MatchResponse>>
{
    private readonly IMatchRepository _matches = matches;

    public async Task<Result<IReadOnlyList<MatchResponse>>> HandleAsync(
        ListInProgressMatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Match> inProgress = await _matches.GetInProgressAsync(cancellationToken);

        IReadOnlyList<MatchResponse> responses = [.. inProgress.Select(MatchResponse.From)];

        return Result.Success(responses);
    }
}
