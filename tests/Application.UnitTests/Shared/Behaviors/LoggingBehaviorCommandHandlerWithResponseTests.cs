using Application.Shared.Behaviors;
using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Shared.Behaviors;

public sealed class LoggingBehaviorCommandHandlerWithResponseTests : IDisposable
{
    private static readonly Error CommandError = new("Test.CommandFailed", "The command failed.");

    private readonly Mock<ICommandHandler<TestCommandWithResponse, int>> _innerMock = new();

    private readonly LoggingBehavior.CommandHandler<TestCommandWithResponse, int> _sut;

    public LoggingBehaviorCommandHandlerWithResponseTests()
    {
        _sut = new LoggingBehavior.CommandHandler<TestCommandWithResponse, int>(_innerMock.Object, NullLogger<LoggingBehavior.CommandHandler<TestCommandWithResponse, int>>.Instance);
    }

    public void Dispose()
    {
        _innerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_SuccessfulCommand_LogsStartAndCompletion()
    {
        // Arrange
        var command = new TestCommandWithResponse();
        Result<int> expected = Result.Success(7);
        CancellationToken token = TestContext.Current.CancellationToken;
        _innerMock.Setup(h => h.HandleAsync(command, token)).ReturnsAsync(expected);

        // Act
        Result<int> result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.Same(expected, result);
        _innerMock.Verify(h => h.HandleAsync(command, token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_FailedCommand_LogsAWarningWithTheError()
    {
        // Arrange
        var command = new TestCommandWithResponse();
        Result<int> expected = Result.Failure<int>(CommandError);
        CancellationToken token = TestContext.Current.CancellationToken;
        _innerMock.Setup(h => h.HandleAsync(command, token)).ReturnsAsync(expected);

        // Act
        Result<int> result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.Same(expected, result);
        _innerMock.Verify(h => h.HandleAsync(command, token), Times.Once());
    }
}
