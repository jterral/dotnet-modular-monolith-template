using Domain.Shared;

namespace Domain.Foosball.Matches;

public sealed record MatchStarted(Guid MatchId, Guid RedPlayerId, Guid BluePlayerId) : DomainEvent;

public sealed record GoalScored(Guid MatchId, Side Side, int RedScore, int BlueScore) : DomainEvent;

public sealed record MatchFinished(Guid MatchId, Guid WinnerId, Guid LoserId, int RedScore, int BlueScore) : DomainEvent;

// A 10-0: the loser never scored. Raised on top of MatchFinished so it can be celebrated on its own.
public sealed record FannyAchieved(Guid MatchId, Guid WinnerId, Guid LoserId) : DomainEvent;
