using Application.Foosball.Matches;
using Application.Foosball.Matches.EventHandlers;
using Application.Foosball.Matches.GetMatch;
using Application.Foosball.Matches.ListInProgressMatches;
using Application.Foosball.Matches.ScheduleMatch;
using Application.Foosball.Matches.ScoreGoal;
using Application.Foosball.Matches.StartMatch;
using Application.Foosball.Players.Leaderboard;
using Application.Foosball.Players.RegisterPlayer;
using Application.Shared.Extensions;
using Domain.Foosball.Matches;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Foosball;

public static class FoosballApplicationExtensions
{
    public static IServiceCollection AddFoosballApplication(this IServiceCollection services)
    {
        services.AddCommandHandler<RegisterPlayerCommand, Guid, RegisterPlayerCommandHandler>();
        services.AddCommandHandler<ScheduleMatchCommand, Guid, ScheduleMatchCommandHandler>();
        services.AddCommandHandler<StartMatchCommand, StartMatchCommandHandler>();
        services.AddCommandHandler<ScoreGoalCommand, ScoreGoalCommandHandler>();

        services.AddValidator<ScheduleMatchCommand, Guid, ScheduleMatchCommandValidator>();

        services.AddQueryHandler<GetMatchQuery, MatchResponse, GetMatchQueryHandler>();
        services.AddQueryHandler<ListInProgressMatchesQuery, IReadOnlyList<MatchResponse>, ListInProgressMatchesQueryHandler>();
        services.AddQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>, LeaderboardQueryHandler>();

        services.AddDomainEventHandler<GoalScored, LogGoalScoredHandler>();
        services.AddDomainEventHandler<MatchFinished, UpdateRatingsOnMatchFinishedHandler>();
        services.AddDomainEventHandler<FannyAchieved, AnnounceFannyHandler>();

        return services;
    }
}
