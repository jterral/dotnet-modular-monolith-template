using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Scalar.AspNetCore;

namespace Infrastructure.Shared.Api.Extensions;

public static class ApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiDocumentation(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi();
        endpoints.MapScalarApiReference();

        return endpoints;
    }

    public static RouteGroupBuilder MapApiVersion(this IEndpointRouteBuilder endpoints, int majorVersion)
    {
        ApiVersionSet versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(majorVersion))
            .ReportApiVersions()
            .Build();

        return endpoints
            .MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);
    }
}
