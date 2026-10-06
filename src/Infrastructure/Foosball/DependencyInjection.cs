using System.Text.RegularExpressions;
using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Players;
using Application.Shared;
using Application.Shared.Events;
using Infrastructure.Shared.HealthChecks;
using Infrastructure.Foosball.Persistence;
using Infrastructure.Shared.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Foosball;

public static partial class DependencyInjection
{
    public const string ConnectionStringName = "Foosball";

    public static IServiceCollection AddFoosballInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = GetConnectionString(configuration);

        // TryAdd: the dispatcher is shared, every module registering itself must not replace it.
        services.TryAddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddDbContext<FoosballDbContext>(options => options
            .UseSqlite(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IFoosballUnitOfWork>(provider => provider.GetRequiredService<FoosballDbContext>());
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        services.AddSingleton<IModuleInitializer, FoosballModuleInitializer>();

        services.AddHealthChecks()
            .AddSqlite(connectionString, name: "foosball-sqlite", tags: [HealthCheckTags.Ready]);

        return services;
    }

    // Standard ConnectionStrings:Foosball key, read eagerly so a misconfigured host fails at startup.
    private static string GetConnectionString(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is missing.");
        }

        // Deployments replace __PLACEHOLDERS__ in appsettings.json; one left behind would silently create a file named after it.
        if (PlaceholderPattern().IsMatch(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' still contains an unreplaced __PLACEHOLDER__.");
        }

        return connectionString;
    }

    [GeneratedRegex("__[A-Z0-9_]+__")]
    private static partial Regex PlaceholderPattern();
}
