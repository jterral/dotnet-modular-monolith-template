using Application.Shared.Messaging;

namespace Application.Foosball.Matches.ScheduleMatch;

public sealed record ScheduleMatchCommand(Guid RedPlayerId, Guid BluePlayerId) : ICommand<Guid>;
