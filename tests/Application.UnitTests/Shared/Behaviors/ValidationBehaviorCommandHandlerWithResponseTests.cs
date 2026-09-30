using Application.Shared;
using Application.Shared.Behaviors;
using Application.Shared.Messaging;
using Domain.Shared;
using Moq;

namespace Application.UnitTests.Shared.Behaviors;

public sealed class ValidationBehaviorCommandHandlerWithResponseTests : IDisposable
{
    private static readonly Error FirstError = new("Test.First", "The first validator failed.");
    private static readonly Error SecondError = new("Test.Second", "The second validator failed.");

    private readonly Mock<ICommandHandler<TestCommandWithResponse, int>> _innerMock = new();
    private readonly Mock<IValidator<TestCommandWithResponse>> _firstValidatorMock = new();
    private readonly Mock<IValidator<TestCommandWithResponse>> _secondValidatorMock = new();
    private readonly ValidationBehavior.CommandHandler<TestCommandWithResponse, int> _sut;

    public ValidationBehaviorCommandHandlerWithResponseTests()
    {
        _sut = new ValidationBehavior.CommandHandler<TestCommandWithResponse, int>(
            _innerMock.Object,
            [_firstValidatorMock.Object, _secondValidatorMock.Object]);
    }

    public void Dispose()
    {
        _innerMock.VerifyNoOtherCalls();
        _firstValidatorMock.VerifyNoOtherCalls();
        _secondValidatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_AllValidatorsSucceed_ReturnsTheInnerResult()
    {
        // Arrange
        var command = new TestCommandWithResponse();
        Result<int> expected = Result.Success(7);
        CancellationToken token = TestContext.Current.CancellationToken;
        _firstValidatorMock.Setup(v => v.ValidateAsync(command, token)).ReturnsAsync(Result.Success());
        _secondValidatorMock.Setup(v => v.ValidateAsync(command, token)).ReturnsAsync(Result.Success());
        _innerMock.Setup(h => h.HandleAsync(command, token)).ReturnsAsync(expected);

        // Act
        Result<int> result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.Same(expected, result);
        _firstValidatorMock.Verify(v => v.ValidateAsync(command, token), Times.Once());
        _secondValidatorMock.Verify(v => v.ValidateAsync(command, token), Times.Once());
        _innerMock.Verify(h => h.HandleAsync(command, token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_FirstValidatorFails_ReturnsFailedResultWithoutRunningTheRest()
    {
        // Arrange
        var command = new TestCommandWithResponse();
        CancellationToken token = TestContext.Current.CancellationToken;
        _firstValidatorMock.Setup(v => v.ValidateAsync(command, token)).ReturnsAsync(Result.Failure(FirstError));

        // Act
        Result<int> result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(FirstError, result.Error);
        _firstValidatorMock.Verify(v => v.ValidateAsync(command, token), Times.Once());
        _secondValidatorMock.Verify(v => v.ValidateAsync(It.IsAny<TestCommandWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
        _innerMock.Verify(h => h.HandleAsync(It.IsAny<TestCommandWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task HandleAsync_SecondValidatorFails_ReturnsFailedResultWithoutCallingTheHandler()
    {
        // Arrange
        var command = new TestCommandWithResponse();
        CancellationToken token = TestContext.Current.CancellationToken;
        _firstValidatorMock.Setup(v => v.ValidateAsync(command, token)).ReturnsAsync(Result.Success());
        _secondValidatorMock.Setup(v => v.ValidateAsync(command, token)).ReturnsAsync(Result.Failure(SecondError));

        // Act
        Result<int> result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SecondError, result.Error);
        _firstValidatorMock.Verify(v => v.ValidateAsync(command, token), Times.Once());
        _secondValidatorMock.Verify(v => v.ValidateAsync(command, token), Times.Once());
        _innerMock.Verify(h => h.HandleAsync(It.IsAny<TestCommandWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
