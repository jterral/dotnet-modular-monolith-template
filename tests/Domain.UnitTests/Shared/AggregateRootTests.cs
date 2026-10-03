using Domain.Shared;

namespace Domain.UnitTests.Shared;

public sealed class AggregateRootTests
{
    [Fact]
    public void DomainEvents_NewAggregate_IsEmpty()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Raise_SeveralEvents_KeepsThemInRaisedOrder()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var first = new FirstEvent();
        var second = new SecondEvent();

        // Act
        aggregate.RaiseEvent(first);
        aggregate.RaiseEvent(second);

        // Assert
        Assert.Equal<IDomainEvent>([first, second], aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_AfterRaise_EmptiesDomainEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.RaiseEvent(new FirstEvent());

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_ViewHeldBeforeClear_ReflectsTheClear()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.RaiseEvent(new FirstEvent());
        IReadOnlyList<IDomainEvent> heldView = aggregate.DomainEvents;

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        // DomainEvents is a live view, not a copy: callers must materialize it before clearing.
        // ApplicationDbContext relies on this, and its comment explains what breaks otherwise.
        Assert.Empty(heldView);
    }

    private sealed record FirstEvent : DomainEvent;

    private sealed record SecondEvent : DomainEvent;

    private sealed class TestAggregate : AggregateRoot
    {
        public void RaiseEvent(IDomainEvent domainEvent)
        {
            Raise(domainEvent);
        }
    }
}
