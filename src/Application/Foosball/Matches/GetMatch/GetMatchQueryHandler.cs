using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;

namespace Application.Foosball.Matches.GetMatch;

internal sealed class GetMatchQueryHandler(IMatchRepository matches) : IQueryHandler<GetMatchQuery, MatchResponse>
{
    private readonly IMatchRepository _matches = matches;

    public async Task<Result<MatchResponse>> HandleAsync(GetMatchQuery query, CancellationToken cancellationToken = default)
    {
        Match? match = await _matches.GetByIdAsync(query.MatchId, cancellationToken);

        return match is null
            ? Result.Failure<MatchResponse>(MatchErrors.NotFound)
            : MatchResponse.From(match);
    }
}
