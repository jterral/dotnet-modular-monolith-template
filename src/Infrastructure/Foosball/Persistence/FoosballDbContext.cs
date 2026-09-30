using Application.Foosball;
using Application.Shared.Events;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Infrastructure.Foosball.Persistence.Configurations;
using Infrastructure.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Foosball.Persistence;

internal sealed class FoosballDbContext(
    DbContextOptions<FoosballDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : ApplicationDbContext(options, domainEventsDispatcher), IFoosballUnitOfWork
{
    public DbSet<Player> Players => Set<Player>();

    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlayerConfiguration());
        modelBuilder.ApplyConfiguration(new MatchConfiguration());
    }
}
