using Infrastructure.Shared.Time;

namespace Infrastructure.UnitTests.Shared.Time;

public sealed class ClockTests
{
    private readonly Clock _sut = new();

    [Fact]
    public void UtcNow_Always_IsTheCurrentTimeInUtc()
    {
        // Arrange
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        DateTimeOffset now = _sut.UtcNow;

        // Assert
        DateTimeOffset after = DateTimeOffset.UtcNow;
        Assert.InRange(now, before, after);
        Assert.Equal(TimeSpan.Zero, now.Offset);
    }
}
