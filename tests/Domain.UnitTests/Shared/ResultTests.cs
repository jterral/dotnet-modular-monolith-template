using Domain.Shared;

namespace Domain.UnitTests.Shared;

public sealed class ResultTests
{
    private static readonly Error SomeError = new("Test.Failure", "Something failed.");

    [Fact]
    public void Success_WithoutValue_IsSuccessWithoutError()
    {
        // Act
        Result result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_WithError_IsFailureCarryingTheError()
    {
        // Act
        Result result = Result.Failure(SomeError);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(SomeError, result.Error);
    }

    [Fact]
    public void Failure_WithNoneError_Throws() =>
        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => Result.Failure(Error.None));

    [Fact]
    public void Failure_WithNullError_Throws() =>
        // Act / Assert
        // Null is passed on purpose to exercise the guard; the compiler would otherwise reject it.
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));

    [Fact]
    public void SuccessOfT_WithValue_ExposesTheValue()
    {
        // Act
        Result<int> result = Result.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureOfT_WithError_IsFailureCarryingTheError()
    {
        // Act
        Result<int> result = Result.Failure<int>(SomeError);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SomeError, result.Error);
    }

    [Fact]
    public void FailureOfT_ReadingValue_Throws()
    {
        // Arrange
        Result<int> result = Result.Failure<int>(SomeError);

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void FailureOfT_WithNoneError_Throws() =>
        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => Result.Failure<int>(Error.None));

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessfulResult()
    {
        // Act
        Result<string> result = "value";

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("value", result.Value);
    }
}
