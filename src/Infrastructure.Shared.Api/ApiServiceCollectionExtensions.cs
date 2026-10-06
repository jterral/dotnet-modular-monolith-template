using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared.Api;

public static class ApiServiceCollectionExtensions
{
    private const int DefaultMajorVersion = 1;

    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services,
        params int[] majorVersions)
    {
        int[] versions = majorVersions.Length > 0 ? majorVersions : [DefaultMajorVersion];

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(versions[0]);
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                // The group name is also the OpenAPI document name, which is how an endpoint lands in its version's document.
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        foreach (int version in versions)
        {
            services.AddOpenApi($"v{version}");
        }

        return services;
    }
}
