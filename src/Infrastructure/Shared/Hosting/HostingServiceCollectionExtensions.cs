using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Shared.Hosting;

public static class HostingServiceCollectionExtensions
{
    public static IServiceCollection AddScopedExecutor(this IServiceCollection services)
    {
        services.TryAddSingleton<ScopedExecutor>();

        return services;
    }
}
