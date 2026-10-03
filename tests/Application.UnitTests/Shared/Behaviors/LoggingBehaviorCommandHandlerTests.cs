using Application.Shared.Behaviors;
using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Shared.Behaviors;

public sealed class LoggingBehaviorCommandHandlerTests : IDisposable
{
    private static readonly Error CommandError = new("Test.CommandFailed", "The command failed.");

    private readonly Mock<ICommandHandler<TestCommand>> _innerMock = new();

    private readonly LoggingBehavior.CommandHandler<TestCommand> _sut;

    public LoggingBehaviorCommandHandlerTests()
    {
        _sut = new LoggingBehavior.CommandHandler<TestCommand>(_innerMock.Object, NullLogger<LoggingBehavior.CommandHandler<TestCommand>>.Instance);
    }

    public void Dispose()
    {
        _innerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_SuccessfulCommand_LogsStartAndCompletion()
    {
        // Arrange
        var command = new TestCommand();
        Result expected = Result.Success();
        CancellationToken token = TestContext.Current.CancellationToken;
        _innerMock.Setup(h => h.HandleAsync(command, token)).ReturnsAsync(expected);

        // Act
        Result result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.Same(expected, result);
        _innerMock.Verify(h => h.HandleAsync(command, token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_FailedCommand_LogsAWarningWithTheError()
    {
        // Arrange
        var command = new TestCommand();
        Result expected = Result.Failure(CommandError);
        CancellationToken token = TestContext.Current.CancellationToken;
        _innerMock.Setup(h => h.HandleAsync(command, token)).ReturnsAsync(expected);

        // Act
        Result result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.Same(expected, result);
        _innerMock.Verify(h => h.HandleAsync(command, token), Times.Once());
    }
}
