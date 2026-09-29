namespace Domain.Shared;

public abstract record DomainEvent : IDomainEvent
{
    // Version 7 GUIDs are time-ordered, avoiding index fragmentation unlike NewGuid().
    public Guid EventId { get; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredAt { get; } = DateTime.UtcNow;
}
