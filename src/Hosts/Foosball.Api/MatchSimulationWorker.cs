using Application.Foosball.Matches;
using Application.Foosball.Matches.ListInProgressMatches;
using Application.Foosball.Matches.ScheduleMatch;
using Application.Foosball.Matches.ScoreGoal;
using Application.Foosball.Matches.StartMatch;
using Application.Foosball.Players.Leaderboard;
using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Shared;
using Infrastructure.Shared.Hosting;
using Microsoft.Extensions.Options;

namespace Foosball.Api;

/// <summary>
/// The robot referee: each tick it plays one goal in every match in progress,
/// and kicks off a new match between two random players when the table is free.
/// </summary>
internal sealed class MatchSimulationWorker(
    ScopedExecutor scopedExecutor,
    IOptions<MatchSimulationOptions> options,
    ILogger<MatchSimulationWorker> logger) : BackgroundService
{
    private const int PlayersPoolSize = 50;

    private readonly ScopedExecutor _scopedExecutor = scopedExecutor;
    private readonly IOptions<MatchSimulationOptions> _options = options;
    private readonly ILogger<MatchSimulationWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Value.TickInterval);

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
            _logger.LogError(exception, "Match simulation tick failed");
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

    private async Task<IReadOnlyList<MatchResponse>> ListInProgressMatchesAsync(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<MatchResponse>> result = await _scopedExecutor.ExecuteAsync<IQueryHandler<ListInProgressMatchesQuery, IReadOnlyList<MatchResponse>>, Result<IReadOnlyList<MatchResponse>>>(
            (handler, token) => handler.HandleAsync(new ListInProgressMatchesQuery(), token), cancellationToken);

        return result.IsSuccess ? result.Value : [];
    }

    private async Task ScoreRandomGoalAsync(Guid matchId, CancellationToken cancellationToken)
    {
        Side side = Random.Shared.Next(2) == 0 ? Side.Red : Side.Blue;

        // Failures are already logged by the logging behavior of the pipeline.
        await _scopedExecutor.ExecuteAsync<ICommandHandler<ScoreGoalCommand>>(
            (handler, token) => handler.HandleAsync(new ScoreGoalCommand(matchId, side), token),
            cancellationToken);
    }

    private async Task StartNewMatchAsync(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LeaderboardEntry>> players = await _scopedExecutor.ExecuteAsync<
            IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>>,
            Result<IReadOnlyList<LeaderboardEntry>>>(
            (handler, token) => handler.HandleAsync(new LeaderboardQuery(PlayersPoolSize), token),
            cancellationToken);

        if (players.IsFailure || players.Value.Count < 2)
        {
            _logger.LogWarning("Not enough players to start a match");

            return;
        }

        IReadOnlyList<LeaderboardEntry> pool = players.Value;
        int redIndex = Random.Shared.Next(pool.Count);
        // Drawing among the remaining players guarantees two distinct ones.
        int blueIndex = (redIndex + 1 + Random.Shared.Next(pool.Count - 1)) % pool.Count;

        // Each handler saves on its own, so scheduling and starting are two units of work, hence two scopes.
        Result<Guid> scheduled = await _scopedExecutor.ExecuteAsync<
            ICommandHandler<ScheduleMatchCommand, Guid>,
            Result<Guid>>(
            (handler, token) => handler.HandleAsync(
                new ScheduleMatchCommand(pool[redIndex].PlayerId, pool[blueIndex].PlayerId),
                token),
            cancellationToken);

        if (scheduled.IsFailure)
        {
            return;
        }

        Result started = await _scopedExecutor.ExecuteAsync<ICommandHandler<StartMatchCommand>, Result>(
            (handler, token) => handler.HandleAsync(new StartMatchCommand(scheduled.Value), token),
            cancellationToken);

        if (started.IsFailure)
        {
            return;
        }

        _logger.LogInformation("Kick-off: {Red} vs {Blue}", pool[redIndex].Name, pool[blueIndex].Name);
    }
}
