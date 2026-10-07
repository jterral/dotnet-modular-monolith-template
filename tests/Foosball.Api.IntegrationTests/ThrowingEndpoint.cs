using Infrastructure.Shared.Api.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Foosball.Api.IntegrationTests;

/// <summary>
/// A route that always fails, to prove the exception handler is wired into the real pipeline.
/// </summary>
internal sealed class ThrowingEndpoint : IEndpoint
{
    public const string Path = "/boom";

    public const string Secret = "secret-internal-detail";

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(Path, IResult () => throw new InvalidOperationException(Secret));
}
