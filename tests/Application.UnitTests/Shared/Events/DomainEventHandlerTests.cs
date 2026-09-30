using Application.Shared.Events;
using Domain.Shared;

namespace Application.UnitTests.Shared.Events;

public sealed class DomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_ThroughTheNonGenericInterface_CallsTheTypedHandler()
    {
        // Arrange
        var typedHandler = new RecordingHandler();
        IDomainEventHandler sut = typedHandler;
        var domainEvent = new FirstEvent();

        // Act
        await sut.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(domainEvent, Assert.Single(typedHandler.Handled));
    }

    [Fact]
    public async Task HandleAsync_ThroughTheNonGenericInterfaceWithAnotherEventType_ThrowsInvalidCast()
    {
        // Arrange
        IDomainEventHandler sut = new RecordingHandler();

        // Act / Assert
        await Assert.ThrowsAsync<InvalidCastException>(() =>
            sut.HandleAsync(new SecondEvent(), TestContext.Current.CancellationToken));
    }

    private sealed record FirstEvent : DomainEvent;

    private sealed record SecondEvent : DomainEvent;

    // A real class rather than a Moq mock: Moq would intercept the default interface method under test.
    private sealed class RecordingHandler : IDomainEventHandler<FirstEvent>
    {
        public List<FirstEvent> Handled { get; } = [];

        public Task HandleAsync(FirstEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Handled.Add(domainEvent);

            return Task.CompletedTask;
        }
    }
}
