using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Shared.Logging;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";

        return services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("ApplicationVersion", version));
    }
}
