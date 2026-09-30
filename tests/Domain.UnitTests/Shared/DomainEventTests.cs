using Domain.Shared;

namespace Domain.UnitTests.Shared;

public sealed class DomainEventTests
{
    [Fact]
    public void EventId_NewEvent_IsAVersion7Guid()
    {
        // Act
        var domainEvent = new TestEvent();

        // Assert
        Assert.Equal(7, domainEvent.EventId.Version);
    }

    [Fact]
    public void EventId_TwoEvents_AreDistinct()
    {
        // Act
        var first = new TestEvent();
        var second = new TestEvent();

        // Assert
        Assert.NotEqual(first.EventId, second.EventId);
    }

    [Fact]
    public void OccurredAt_NewEvent_IsTheCreationTimeInUtc()
    {
        // Arrange
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        var domainEvent = new TestEvent();

        // Assert
        DateTimeOffset after = DateTimeOffset.UtcNow;
        Assert.InRange(domainEvent.OccurredAt, before, after);
        Assert.Equal(TimeSpan.Zero, domainEvent.OccurredAt.Offset);
    }

    private sealed record TestEvent : DomainEvent;
}
