using Application.Shared;
using Application.Shared.Extensions;
using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests.Shared.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    private static readonly Error NameRequired = new("Test.NameRequired", "Name is required.");

    [Fact]
    public async Task AddValidator_CommandWithResponseAndInvalidInput_ReturnsValidationError()
    {
        // Arrange
        await using ServiceProvider provider = BuildProvider(services => services
            .AddCommandHandler<CreateCommand, Guid, CreateCommandHandler>()
            .AddValidator<CreateCommand, Guid, CreateCommandValidator>());
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICommandHandler<CreateCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateCommand, Guid>>();

        // Act
        Result<Guid> result = await handler.HandleAsync(
            new CreateCommand(string.Empty),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(NameRequired, result.Error);
    }

    [Fact]
    public async Task AddValidator_CommandWithResponseAndValidInput_ReturnsHandlerResult()
    {
        // Arrange
        await using ServiceProvider provider = BuildProvider(services => services
            .AddCommandHandler<CreateCommand, Guid, CreateCommandHandler>()
            .AddValidator<CreateCommand, Guid, CreateCommandValidator>());
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICommandHandler<CreateCommand, Guid> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateCommand, Guid>>();

        // Act
        Result<Guid> result = await handler.HandleAsync(
            new CreateCommand("order"),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task AddValidator_CommandWithoutResponseAndInvalidInput_ReturnsValidationError()
    {
        // Arrange
        await using ServiceProvider provider = BuildProvider(services => services
            .AddCommandHandler<RenameCommand, RenameCommandHandler>()
            .AddValidator<RenameCommand, RenameCommandValidator>());
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ICommandHandler<RenameCommand> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<RenameCommand>>();

        // Act
        Result result = await handler.HandleAsync(
            new RenameCommand(string.Empty),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(NameRequired, result.Error);
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();

        // The behaviors resolve ILogger<T> for internal decorator types; a null logger is enough here.
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        configure(services);

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private sealed record CreateCommand(string Name) : ICommand<Guid>;

    private sealed class CreateCommandHandler : ICommandHandler<CreateCommand, Guid>
    {
        public Task<Result<Guid>> HandleAsync(CreateCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(Guid.CreateVersion7()));
    }

    private sealed class CreateCommandValidator : IValidator<CreateCommand>
    {
        public Task<Result> ValidateAsync(CreateCommand instance, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.IsNullOrEmpty(instance.Name) ? Result.Failure(NameRequired) : Result.Success());
    }

    private sealed record RenameCommand(string Name) : ICommand;

    private sealed class RenameCommandHandler : ICommandHandler<RenameCommand>
    {
        public Task<Result> HandleAsync(RenameCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class RenameCommandValidator : IValidator<RenameCommand>
    {
        public Task<Result> ValidateAsync(RenameCommand instance, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.IsNullOrEmpty(instance.Name) ? Result.Failure(NameRequired) : Result.Success());
    }
}
