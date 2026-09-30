using Domain.Shared;

namespace Domain.Foosball.Matches;

public sealed class Match : AggregateRoot
{
    public const int WinningScore = 10;

    // Required by EF Core to materialize the entity.
    private Match()
    {
    }

    public Guid RedPlayerId { get; private set; }

    public Guid BluePlayerId { get; private set; }

    public int RedScore { get; private set; }

    public int BlueScore { get; private set; }

    public MatchStatus Status { get; private set; } = MatchStatus.Scheduled;

    public Guid? WinnerId { get; private set; }

    public static Result<Match> Schedule(Guid redPlayerId, Guid bluePlayerId)
    {
        if (redPlayerId == bluePlayerId)
        {
            return Result.Failure<Match>(MatchErrors.SamePlayer);
        }

        return new Match { RedPlayerId = redPlayerId, BluePlayerId = bluePlayerId };
    }

    public Result Start()
    {
        if (Status != MatchStatus.Scheduled)
        {
            return Result.Failure(MatchErrors.NotScheduled);
        }

        Status = MatchStatus.InProgress;
        Raise(new MatchStarted(Id, RedPlayerId, BluePlayerId));

        return Result.Success();
    }

    public Result ScoreGoal(Side side)
    {
        if (Status != MatchStatus.InProgress)
        {
            return Result.Failure(MatchErrors.NotInProgress);
        }

        if (side == Side.Red)
        {
            RedScore++;
        }
        else
        {
            BlueScore++;
        }

        Raise(new GoalScored(Id, side, RedScore, BlueScore));

        if (RedScore == WinningScore || BlueScore == WinningScore)
        {
            Finish();
        }

        return Result.Success();
    }

    private void Finish()
    {
        bool redWon = RedScore > BlueScore;
        Guid winnerId = redWon ? RedPlayerId : BluePlayerId;
        Guid loserId = redWon ? BluePlayerId : RedPlayerId;

        Status = MatchStatus.Finished;
        WinnerId = winnerId;

        Raise(new MatchFinished(Id, winnerId, loserId, RedScore, BlueScore));

        if (Math.Min(RedScore, BlueScore) == 0)
        {
            Raise(new FannyAchieved(Id, winnerId, loserId));
        }
    }
}
