using System.Collections.Concurrent;
using Application.Shared.Events;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Shared.DomainEvents;

internal sealed class DomainEventsDispatcher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventsDispatcher> logger) : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypes = new();

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type eventType = domainEvent.GetType();

            Type handlerType = HandlerTypes.GetOrAdd(
                eventType,
                static type => typeof(IDomainEventHandler<>).MakeGenericType(type));

            foreach (IDomainEventHandler handler in serviceProvider
                         .GetServices(handlerType)
                         .OfType<IDomainEventHandler>())
            {
                logger.LogInformation(
                    "Handling {DomainEvent} with {Handler}",
                    eventType.Name,
                    handler.GetType().Name);

                try
                {
                    await handler.HandleAsync(domainEvent, cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Handler failed for {DomainEvent}", eventType.Name);
                    throw;
                }
            }
        }
    }
}
