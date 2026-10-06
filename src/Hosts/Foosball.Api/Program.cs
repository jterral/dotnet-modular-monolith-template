using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Matches.ListInProgressMatches;
using Application.Foosball.Players.Leaderboard;
using Application.Shared.Messaging;
using Domain.Shared;
using Foosball.Api;
using Foosball.Api.Jobs;
using Infrastructure.Foosball;
using Infrastructure.Shared.Api;
using Infrastructure.Shared.HealthChecks;
using Infrastructure.Shared.Hosting;
using Infrastructure.Shared.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// ----------
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogLogging(builder.Configuration);
builder.Services.AddFoosballApplication();
builder.Services.AddFoosballInfrastructure(builder.Configuration);

// The referee has no request scope: it resolves its handlers through ScopedExecutor.
builder.Services.AddScopedExecutor();
builder.Services
    .AddOptions<MatchSimulationOptions>()
    .Bind(builder.Configuration.GetSection(MatchSimulationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHostedService<MatchSimulationJob>();

builder.Services.AddApiDocumentation();

// ----------
WebApplication app = builder.Build();

app.UsePathBase("/foosball");
app.UseRouting();

// Live runs no check (the process answers); ready runs the checks tagged Ready (database).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckTags.Ready) });

app.MapApiDocumentation();

RouteGroupBuilder v1 = app.MapApiVersion(1);

// Matches in progress when there are any, the players (by rating) otherwise.
v1.MapGet("/foosball", async (
    IQueryHandler<ListInProgressMatchesQuery, IReadOnlyList<MatchResponse>> matchesHandler,
    IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>> leaderboardHandler,
    CancellationToken cancellationToken) =>
{
    Result<IReadOnlyList<MatchResponse>> matches = await matchesHandler.HandleAsync(new ListInProgressMatchesQuery(), cancellationToken);
    if (matches.IsFailure)
    {
        return Results.Problem(matches.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
    }

    if (matches.Value.Count > 0)
    {
        return Results.Ok(new FoosballOverview("matches", matches.Value, []));
    }

    Result<IReadOnlyList<LeaderboardEntry>> players = await leaderboardHandler.HandleAsync(new LeaderboardQuery(int.MaxValue), cancellationToken);
    return players.IsFailure
        ? Results.Problem(players.Error.Description, statusCode: StatusCodes.Status500InternalServerError)
        : Results.Ok(new FoosballOverview("players", [], players.Value));
})
.WithSummary("Returns the matches in progress, or the players when no match is in progress.")
.Produces<FoosballOverview>()
.ProducesProblem(StatusCodes.Status500InternalServerError);

await app.RunAsync();
