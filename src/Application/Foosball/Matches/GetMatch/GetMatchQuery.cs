using Application.Shared.Messaging;

namespace Application.Foosball.Matches.GetMatch;

public sealed record GetMatchQuery(Guid MatchId) : IQuery<MatchResponse>;
