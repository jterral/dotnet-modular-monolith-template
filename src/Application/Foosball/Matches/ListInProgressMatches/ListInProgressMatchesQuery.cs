using Application.Shared.Messaging;

namespace Application.Foosball.Matches.ListInProgressMatches;

public sealed record ListInProgressMatchesQuery : IQuery<IReadOnlyList<MatchResponse>>;
