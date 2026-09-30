using Application.Shared.Events;
using Infrastructure.Shared.DomainEvents;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.DbTests.Shared.Persistence;

public sealed class ApplicationDbContextTests : IAsyncLifetime
{
    // An in-memory SQLite database lives only as long as its connection, so one is kept open for the whole test.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly HandlerBehavior _behavior = new();
    // Assigned in InitializeAsync, which xUnit always runs before a test.
    private ServiceProvider _provider = null!;

    public async ValueTask InitializeAsync()
    {
        await _connection.OpenAsync(TestContext.Current.CancellationToken);

        _provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddSingleton(_behavior)
            .AddDbContext<TestDbContext>(options => options.UseSqlite(_connection))
            .AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>()
            .AddScoped<IDomainEventHandler<OrderCreated>, OrderCreatedHandler>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestDbContext>()
            .Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_NewAggregateWithEvent_DispatchesAfterTheRowIsPersisted()
    {
        // Arrange
        int rowsSeenByHandler = -1;
        _behavior.OnOrderCreated = async (context, _, cancellationToken) =>
            rowsSeenByHandler = await context.Orders.AsNoTracking().CountAsync(cancellationToken);
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var order = Order.Create("original");
        context.Orders.Add(order);

        // Act
        int affected = await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, affected);
        Assert.Equal(1, rowsSeenByHandler);
        Assert.Equal(order.Id, Assert.Single(_behavior.Handled).OrderId);
    }

    [Fact]
    public async Task SaveChangesAsync_AggregateWithEvent_ClearsTheEventsFromTheAggregate()
    {
        // Arrange
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var order = Order.Create("original");
        context.Orders.Add(order);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(order.DomainEvents);
        Assert.Single(_behavior.Handled);
    }

    [Fact]
    public async Task SaveChangesAsync_TwoAggregates_DispatchesTheEventOfEach()
    {
        // Arrange
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var first = Order.Create("first");
        var second = Order.Create("second");
        context.Orders.AddRange(first, second);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equivalent(new[] { first.Id, second.Id }, _behavior.Handled.Select(e => e.OrderId));
    }

    [Fact]
    public async Task SaveChangesAsync_NoChanges_ReturnsZeroAndDispatchesNothing()
    {
        // Arrange
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        int affected = await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, affected);
        Assert.Empty(_behavior.Handled);
    }

    [Fact]
    public async Task SaveChangesAsync_HandlerChangesAnEntityWithoutSaving_TheChangeIsNotPersisted()
    {
        // Arrange
        _behavior.OnOrderCreated = async (context, domainEvent, cancellationToken) =>
        {
            Order tracked = await context.Orders.SingleAsync(o => o.Id == domainEvent.OrderId, cancellationToken);
            tracked.Rename("renamed by handler");
        };
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var order = Order.Create("original");
        context.Orders.Add(order);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        // Nothing saves on the handler's behalf: the rename is only pending in the change tracker.
        Assert.True(context.ChangeTracker.HasChanges());
        Assert.Equal("original", await ReadNameAsync(order.Id));
    }

    [Fact]
    public async Task SaveChangesAsync_HandlerSavesItsOwnChange_TheChangeIsPersisted()
    {
        // Arrange
        _behavior.OnOrderCreated = async (context, domainEvent, cancellationToken) =>
        {
            Order tracked = await context.Orders.SingleAsync(o => o.Id == domainEvent.OrderId, cancellationToken);
            tracked.Rename("renamed by handler");
            await context.SaveChangesAsync(cancellationToken);
        };
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var order = Order.Create("original");
        context.Orders.Add(order);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("renamed by handler", await ReadNameAsync(order.Id));
    }

    [Fact]
    public async Task SaveChangesAsync_HandlerThrows_TheDataStaysPersistedAndTheExceptionReachesTheCaller()
    {
        // Arrange
        var failure = new InvalidOperationException("The handler failed.");
        _behavior.OnOrderCreated = (_, _, _) => throw failure;
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var order = Order.Create("original");
        context.Orders.Add(order);

        // Act
        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));

        // Assert
        // Events are dispatched after the commit: the row is saved and the events are not replayed.
        Assert.Same(failure, thrown);
        Assert.Equal("original", await ReadNameAsync(order.Id));
        Assert.Empty(order.DomainEvents);
    }

    // Reads through a new scope, hence a new DbContext, so only committed data is visible.
    private async Task<string> ReadNameAsync(Guid orderId)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        return await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => o.Name)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    private sealed class HandlerBehavior
    {
        public List<OrderCreated> Handled { get; } = [];

        public Func<TestDbContext, OrderCreated, CancellationToken, Task> OnOrderCreated { get; set; } =
            (_, _, _) => Task.CompletedTask;
    }

    private sealed class OrderCreatedHandler(TestDbContext context, HandlerBehavior behavior)
        : IDomainEventHandler<OrderCreated>
    {
        public async Task HandleAsync(OrderCreated domainEvent, CancellationToken cancellationToken = default)
        {
            behavior.Handled.Add(domainEvent);

            await behavior.OnOrderCreated(context, domainEvent, cancellationToken);
        }
    }
}
