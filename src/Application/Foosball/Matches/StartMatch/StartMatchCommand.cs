using Application.Shared.Messaging;

namespace Application.Foosball.Matches.StartMatch;

public sealed record StartMatchCommand(Guid MatchId) : ICommand;
