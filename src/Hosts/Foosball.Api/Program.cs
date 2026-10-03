using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Matches.ListInProgressMatches;
using Application.Foosball.Players.Leaderboard;
using Application.Shared;
using Application.Shared.Messaging;
using Domain.Shared;
using Foosball.Api;
using Infrastructure.Foosball;
using Infrastructure.Shared.Logging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogLogging(builder.Configuration);
builder.Services.AddFoosballApplication();
builder.Services.AddFoosballInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

// Creates the schema when the API starts before the Worker (EnsureCreated and the seed are idempotent).
foreach (IModuleInitializer initializer in app.Services.GetServices<IModuleInitializer>())
{
    await initializer.InitialiseAsync();
}

// Matches in progress when there are any, the players (by rating) otherwise.
app.MapGet("/foosball", async (
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
