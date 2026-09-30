using Application.Shared.Messaging;
using Domain.Foosball.Matches;

namespace Application.Foosball.Matches.ScoreGoal;

public sealed record ScoreGoalCommand(Guid MatchId, Side Side) : ICommand;
