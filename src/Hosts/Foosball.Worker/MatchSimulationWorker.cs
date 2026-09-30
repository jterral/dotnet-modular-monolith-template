using Application.Foosball.Matches;
using Application.Foosball.Matches.ListInProgressMatches;
using Application.Foosball.Matches.ScheduleMatch;
using Application.Foosball.Matches.ScoreGoal;
using Application.Foosball.Matches.StartMatch;
using Application.Foosball.Players.Leaderboard;
using Application.Shared;
using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;
using Microsoft.Extensions.Options;

namespace Foosball.Worker;

/// <summary>
/// The robot referee: each tick it plays one goal in every match in progress,
/// and kicks off a new match between two random players when the table is free.
/// </summary>
internal sealed class MatchSimulationWorker(
    UnitOfWorkScope unitOfWork,
    IOptions<MatchSimulationOptions> options,
    ILogger<MatchSimulationWorker> logger) : BackgroundService
{
    private const int PlayersPoolSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.TickInterval);

        try
        {
            do
            {
                await TickSafelyAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
    }

    private async Task TickSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await TickAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // A failing tick must not stop the referee: the next one starts from a fresh scope.
            logger.LogError(exception, "Match simulation tick failed");
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<MatchResponse> inProgress = await ListInProgressMatchesAsync(cancellationToken);

        if (inProgress.Count == 0)
        {
            await StartNewMatchAsync(cancellationToken);

            return;
        }

        foreach (MatchResponse match in inProgress)
        {
            await ScoreRandomGoalAsync(match.Id, cancellationToken);
        }
    }

    private Task<IReadOnlyList<MatchResponse>> ListInProgressMatchesAsync(CancellationToken cancellationToken) =>
        unitOfWork.ExecuteAsync<IReadOnlyList<MatchResponse>>(
            async (services, token) =>
            {
                var handler = services
                    .GetRequiredService<IQueryHandler<ListInProgressMatchesQuery, IReadOnlyList<MatchResponse>>>();
                Result<IReadOnlyList<MatchResponse>> result = await handler.HandleAsync(new ListInProgressMatchesQuery(), token);

                return result.IsSuccess ? result.Value : [];
            },
            cancellationToken);

    private Task ScoreRandomGoalAsync(Guid matchId, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteAsync(
            async (services, token) =>
            {
                var handler = services.GetRequiredService<ICommandHandler<ScoreGoalCommand>>();
                Side side = Random.Shared.Next(2) == 0 ? Side.Red : Side.Blue;

                // Failures are already logged by the logging behavior of the pipeline.
                await handler.HandleAsync(new ScoreGoalCommand(matchId, side), token);
            },
            cancellationToken);

    private Task StartNewMatchAsync(CancellationToken cancellationToken) =>
        unitOfWork.ExecuteAsync(
            async (services, token) =>
            {
                var leaderboard = services
                    .GetRequiredService<IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>>>();
                Result<IReadOnlyList<LeaderboardEntry>> players = await leaderboard.HandleAsync(new LeaderboardQuery(PlayersPoolSize), token);

                if (players.IsFailure || players.Value.Count < 2)
                {
                    logger.LogWarning("Not enough players to start a match");

                    return;
                }

                IReadOnlyList<LeaderboardEntry> pool = players.Value;
                int redIndex = Random.Shared.Next(pool.Count);
                // Drawing among the remaining players guarantees two distinct ones.
                int blueIndex = (redIndex + 1 + Random.Shared.Next(pool.Count - 1)) % pool.Count;

                var schedule = services.GetRequiredService<ICommandHandler<ScheduleMatchCommand, Guid>>();
                Result<Guid> scheduled = await schedule.HandleAsync(
                    new ScheduleMatchCommand(pool[redIndex].PlayerId, pool[blueIndex].PlayerId),
                    token);

                if (scheduled.IsFailure)
                {
                    return;
                }

                var start = services.GetRequiredService<ICommandHandler<StartMatchCommand>>();
                await start.HandleAsync(new StartMatchCommand(scheduled.Value), token);

                logger.LogInformation(
                    "Kick-off: {Red} vs {Blue}",
                    pool[redIndex].Name,
                    pool[blueIndex].Name);
            },
            cancellationToken);
}
