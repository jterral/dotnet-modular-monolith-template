using Infrastructure.Shared.Api.Extensions;
using Microsoft.AspNetCore.Routing;

namespace Infrastructure.Shared.Api.Abstractions;

/// <summary>
/// A group of routes registered on the application by <see cref="EndpointExtensions.MapEndpoints"/>.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the routes of this endpoint.
    /// </summary>
    /// <param name="app">The route builder to map onto.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
