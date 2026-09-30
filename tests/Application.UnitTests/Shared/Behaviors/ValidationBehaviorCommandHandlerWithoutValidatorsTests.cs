using Application.Shared;
using Application.Shared.Behaviors;
using Application.Shared.Messaging;
using Domain.Shared;
using Moq;

namespace Application.UnitTests.Shared.Behaviors;

public sealed class ValidationBehaviorCommandHandlerWithoutValidatorsTests : IDisposable
{
    private readonly Mock<ICommandHandler<TestCommand>> _innerMock = new();
    private readonly ValidationBehavior.CommandHandler<TestCommand> _sut;

    public ValidationBehaviorCommandHandlerWithoutValidatorsTests()
    {
        _sut = new ValidationBehavior.CommandHandler<TestCommand>(_innerMock.Object, []);
    }

    public void Dispose() => _innerMock.VerifyNoOtherCalls();

    [Fact]
    public async Task HandleAsync_NoValidatorRegistered_ReturnsTheInnerResult()
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
}
