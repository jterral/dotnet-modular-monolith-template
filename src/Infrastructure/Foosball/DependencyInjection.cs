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
using Microsoft.Extensions.Options;

namespace Infrastructure.Foosball;

public static class DependencyInjection
{
    public static IServiceCollection AddFoosballInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<FoosballOptions>()
            .Bind(configuration.GetSection(FoosballOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // TryAdd: the dispatcher is shared, every module registering itself must not replace it.
        services.TryAddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddDbContext<FoosballDbContext>((provider, options) => options
            .UseSqlite(provider.GetRequiredService<IOptions<FoosballOptions>>().Value.ConnectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IFoosballUnitOfWork>(provider => provider.GetRequiredService<FoosballDbContext>());
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        services.AddSingleton<IModuleInitializer, FoosballModuleInitializer>();

        return services;
    }
}
