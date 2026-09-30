using Microsoft.Extensions.Logging;
using Moq;

namespace Testing.Shared.Extensions;

public static class LoggerMockExtensions
{
    // ILogger.Log is the only member the LoggerExtensions helpers (LogInformation, LogWarning...) call,
    // so verifying it is enough to assert on a log entry without depending on the exact message template.
    public static void VerifyLog<T>(
        this Mock<ILogger<T>> loggerMock,
        LogLevel level,
        Times times,
        params string[] expectedParts) =>
        loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => ContainsAll(state, expectedParts)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);

    public static void VerifyLog<T>(
        this Mock<ILogger<T>> loggerMock,
        LogLevel level,
        Times times,
        Exception exception,
        params string[] expectedParts) =>
        loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => ContainsAll(state, expectedParts)),
                It.Is<Exception?>(actual => ReferenceEquals(actual, exception)),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);

    // Public because Moq evaluates the expression tree above outside of this assembly.
    public static bool ContainsAll(object state, string[] expectedParts)
    {
        string message = state.ToString() ?? string.Empty;

        return expectedParts.All(part => message.Contains(part, StringComparison.Ordinal));
    }
}
