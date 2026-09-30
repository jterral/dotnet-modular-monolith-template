using Application.Shared;
using Domain.Foosball.Players;
using Infrastructure.Foosball.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Foosball;

internal sealed class FoosballModuleInitializer(IServiceScopeFactory scopeFactory) : IModuleInitializer
{
    private static readonly string[] SeedPlayerNames = ["Zizou", "Platini", "Kopa", "Mbappe"];

    public string ModuleName => "Foosball";

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        FoosballDbContext context = scope.ServiceProvider.GetRequiredService<FoosballDbContext>();

        // No migrations in the template yet: EnsureCreated builds the schema on an empty database.
        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (await context.Players.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (string name in SeedPlayerNames)
        {
            context.Players.Add(Player.Create(PlayerName.Create(name).Value));
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
