using Application.Shared;
using Domain.Foosball.Players;
using Infrastructure.Foosball.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Foosball;

internal sealed class FoosballModuleInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<FoosballModuleInitializer> logger) : IModuleInitializer
{
    private static readonly string[] SeedPlayerNames = ["Zizou", "Platini", "Kopa", "Mbappe"];

    public string ModuleName => "Foosball";

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    private readonly ILogger _logger = logger;

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        FoosballDbContext context = scope.ServiceProvider.GetRequiredService<FoosballDbContext>();

        _logger.LogInformation("Foosball database: {Database}", context.Database.GetDbConnection().DataSource);

        await context.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Foosball migrations applied");

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
