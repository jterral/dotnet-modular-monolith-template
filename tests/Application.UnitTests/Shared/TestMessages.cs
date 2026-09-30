using Application.Shared.Messaging;

namespace Application.UnitTests.Shared;

// Public on purpose: Moq mocks handler interfaces closed over these types, and the proxies live in another assembly.
public sealed record TestCommand : ICommand;

public sealed record TestCommandWithResponse : ICommand<int>;

public sealed record TestQuery : IQuery<int>;
