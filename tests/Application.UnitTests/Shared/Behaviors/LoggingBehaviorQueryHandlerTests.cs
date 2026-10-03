using Application.Shared.Behaviors;
using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Shared.Behaviors;

public sealed class LoggingBehaviorQueryHandlerTests : IDisposable
{
    private static readonly Error QueryError = new("Test.QueryFailed", "The query failed.");

    private readonly Mock<IQueryHandler<TestQuery, int>> _innerMock = new();

    private readonly LoggingBehavior.QueryHandler<TestQuery, int> _sut;

    public LoggingBehaviorQueryHandlerTests()
    {
        _sut = new LoggingBehavior.QueryHandler<TestQuery, int>(_innerMock.Object, NullLogger<LoggingBehavior.QueryHandler<TestQuery, int>>.Instance);
    }

    public void Dispose()
    {
        _innerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_SuccessfulQuery_LogsOnlyTheStart()
    {
        // Arrange
        var query = new TestQuery();
        Result<int> expected = Result.Success(7);
        CancellationToken token = TestContext.Current.CancellationToken;
        _innerMock.Setup(h => h.HandleAsync(query, token)).ReturnsAsync(expected);

        // Act
        Result<int> result = await _sut.HandleAsync(query, token);

        // Assert
        Assert.Same(expected, result);
        _innerMock.Verify(h => h.HandleAsync(query, token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_FailedQuery_LogsAWarningWithTheError()
    {
        // Arrange
        var query = new TestQuery();
        Result<int> expected = Result.Failure<int>(QueryError);
        CancellationToken token = TestContext.Current.CancellationToken;
        _innerMock.Setup(h => h.HandleAsync(query, token)).ReturnsAsync(expected);

        // Act
        Result<int> result = await _sut.HandleAsync(query, token);

        // Assert
        Assert.Same(expected, result);
        _innerMock.Verify(h => h.HandleAsync(query, token), Times.Once());
    }
}
