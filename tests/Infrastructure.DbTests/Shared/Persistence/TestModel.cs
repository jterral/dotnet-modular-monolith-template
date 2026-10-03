using Application.Shared.Events;
using Domain.Shared;
using Infrastructure.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DbTests.Shared.Persistence;

public sealed record OrderCreated(Guid OrderId) : DomainEvent;

public sealed class Order : AggregateRoot
{
    // Required by EF Core to materialize the entity.
    private Order()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public static Order Create(string name)
    {
        var order = new Order { Name = name };
        order.Raise(new OrderCreated(order.Id));

        return order;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}

public sealed class TestDbContext(DbContextOptions<TestDbContext> options, IDomainEventsDispatcher domainEventsDispatcher)
    : ApplicationDbContext(options, domainEventsDispatcher)
{
    public DbSet<Order> Orders => Set<Order>();
}
