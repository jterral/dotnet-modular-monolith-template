using Application.Shared.Messaging;

namespace Application.Foosball.Players.RegisterPlayer;

public sealed record RegisterPlayerCommand(string Name) : ICommand<Guid>;
