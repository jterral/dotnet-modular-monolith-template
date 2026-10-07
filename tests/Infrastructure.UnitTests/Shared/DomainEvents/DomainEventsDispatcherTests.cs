using Application.Shared.Events;
using Domain.Shared;
using Infrastructure.Shared.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.UnitTests.Shared.DomainEvents;

public sealed class DomainEventsDispatcherTests : IDisposable
{
    private readonly List<string> _calls = [];

    private readonly InvalidOperationException _failure = new("The handler failed.");

    private readonly ServiceProvider _provider;

    private readonly DomainEventsDispatcher _sut;

    public DomainEventsDispatcherTests()
    {
        _provider = new ServiceCollection()
            .AddSingleton<IDomainEventHandler<FirstEvent>>(new RecordingHandler<FirstEvent>("A", _calls))
            .AddSingleton<IDomainEventHandler<FirstEvent>>(new RecordingHandler<FirstEvent>("B", _calls))
            .AddSingleton<IDomainEventHandler<SecondEvent>>(new RecordingHandler<SecondEvent>("C", _calls))
            .AddSingleton<IDomainEventHandler<FailingEvent>>(new ThrowingHandler(_failure))
            .AddSingleton<IDomainEventHandler<FailingEvent>>(new RecordingHandler<FailingEvent>("D", _calls))
            .BuildServiceProvider();

        _sut = new DomainEventsDispatcher(_provider, NullLogger<DomainEventsDispatcher>.Instance);
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task DispatchAsync_EventWithTwoHandlers_InvokesBothInRegistrationOrder()
    {
        // Act
        await _sut.DispatchAsync([new FirstEvent()], TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["A:FirstEvent", "B:FirstEvent"], _calls);
    }

    [Fact]
    public async Task DispatchAsync_SeveralEvents_RoutesEachEventToItsOwnHandlers()
    {
        // Act
        await _sut.DispatchAsync([new FirstEvent(), new SecondEvent()], TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["A:FirstEvent", "B:FirstEvent", "C:SecondEvent"], _calls);
    }

    [Fact]
    public async Task DispatchAsync_EventWithoutHandler_DoesNothing()
    {
        // Act
        await _sut.DispatchAsync([new UnhandledEvent()], TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(_calls);
    }

    [Fact]
    public async Task DispatchAsync_NoEvents_DoesNothing()
    {
        // Act
        await _sut.DispatchAsync([], TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(_calls);
    }

    [Fact]
    public async Task DispatchAsync_HandlerThrows_LogsTheErrorAndRethrowsWithoutRunningLaterHandlers()
    {
        // Act
        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DispatchAsync([new FailingEvent()], TestContext.Current.CancellationToken));

        // Assert
        Assert.Same(_failure, thrown);
        Assert.Empty(_calls);
    }

    private sealed record FirstEvent : DomainEvent;

    private sealed record SecondEvent : DomainEvent;

    private sealed record FailingEvent : DomainEvent;

    private sealed record UnhandledEvent : DomainEvent;

    // Real handlers rather than Moq mocks: the dispatcher calls the non-generic default interface method,
    // which Moq would intercept instead of forwarding to the typed handler.
    private sealed class RecordingHandler<TEvent>(string name, List<string> calls) : IDomainEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        private readonly string _name = name;
        private readonly List<string> _calls = calls;

        public Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _calls.Add($"{_name}:{typeof(TEvent).Name}");

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler(Exception failure) : IDomainEventHandler<FailingEvent>
    {
        private readonly Exception _failure = failure;

        public Task HandleAsync(FailingEvent domainEvent, CancellationToken cancellationToken = default) =>
            Task.FromException(_failure);
    }
}
