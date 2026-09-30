using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Players;
using Application.Shared;
using Application.Shared.Events;
using Infrastructure.Foosball.Persistence;
using Infrastructure.Shared.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Foosball;

public static class FoosballInfrastructureExtensions
{
    public const string ConnectionStringName = "Foosball";

    private const string DefaultConnectionString = "Data Source=foosball.db";

    public static IServiceCollection AddFoosballInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString(ConnectionStringName) ?? DefaultConnectionString;

        // TryAdd: the dispatcher is shared, every module registering itself must not replace it.
        services.TryAddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddDbContext<FoosballDbContext>(options => options
            .UseSqlite(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IFoosballUnitOfWork>(provider => provider.GetRequiredService<FoosballDbContext>());
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        services.AddSingleton<IModuleInitializer, FoosballModuleInitializer>();

        return services;
    }
}
