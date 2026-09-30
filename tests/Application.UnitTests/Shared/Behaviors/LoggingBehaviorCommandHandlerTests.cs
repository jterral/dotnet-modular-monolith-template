using Application.Shared.Behaviors;
using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using Testing.Shared.Extensions;

namespace Application.UnitTests.Shared.Behaviors;

public sealed class LoggingBehaviorCommandHandlerTests : IDisposable
{
    private static readonly Error CommandError = new("Test.CommandFailed", "The command failed.");

    private readonly Mock<ICommandHandler<TestCommand>> _innerMock = new();
    private readonly Mock<ILogger<LoggingBehavior.CommandHandler<TestCommand>>> _loggerMock = new();
    private readonly LoggingBehavior.CommandHandler<TestCommand> _sut;

    public LoggingBehaviorCommandHandlerTests()
    {
        _sut = new LoggingBehavior.CommandHandler<TestCommand>(_innerMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _innerMock.VerifyNoOtherCalls();
        _loggerMock.VerifyNoOtherCalls();
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
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once(), "Executing command TestCommand");
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once(), "Command TestCommand completed");
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
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once(), "Executing command TestCommand");
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once(), "Command TestCommand failed", CommandError.Code);
    }
}
