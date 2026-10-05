using Application.Foosball.Matches;
using Application.Foosball.Matches.ListInProgressMatches;
using Application.Foosball.Players.Leaderboard;
using Application.Shared.Messaging;
using Domain.Shared;

namespace Foosball.Api.Endpoints;

internal sealed class GetFoosballOverview : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Matches in progress when there are any, the players (by rating) otherwise.
        app.MapGet("/foosball", HandleAsync)
            .WithSummary("Returns the matches in progress, or the players when no match is in progress.")
            .Produces<FoosballOverview>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        IQueryHandler<ListInProgressMatchesQuery, IReadOnlyList<MatchResponse>> matchesHandler,
        IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>> leaderboardHandler,
        CancellationToken cancellationToken)
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
    }
}
