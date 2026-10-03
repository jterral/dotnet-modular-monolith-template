using Domain.Foosball.Matches;
using Domain.Shared;

namespace Domain.UnitTests.Foosball.Matches;

public sealed class MatchTests
{
    private readonly Guid _redId = Guid.CreateVersion7();
    private readonly Guid _blueId = Guid.CreateVersion7();

    [Fact]
    public void Schedule_TwoDifferentPlayers_CreatesAScheduledMatchWithoutScore()
    {
        // Act
        Result<Match> result = Match.Schedule(_redId, _blueId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Scheduled, result.Value.Status);
        Assert.Equal(0, result.Value.RedScore);
        Assert.Equal(0, result.Value.BlueScore);
        Assert.Null(result.Value.WinnerId);
    }

    [Fact]
    public void Schedule_SamePlayerOnBothSides_ReturnsSamePlayerError()
    {
        // Act
        Result<Match> result = Match.Schedule(_redId, _redId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MatchErrors.SamePlayer, result.Error);
    }

    [Fact]
    public void Start_ScheduledMatch_MovesToInProgressAndRaisesMatchStarted()
    {
        // Arrange
        Match match = NewMatch();

        // Act
        Result result = match.Start();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.InProgress, match.Status);
        MatchStarted started = Assert.IsType<MatchStarted>(Assert.Single(match.DomainEvents));
        Assert.Equal(match.Id, started.MatchId);
    }

    [Fact]
    public void Start_MatchAlreadyStarted_ReturnsNotScheduledError()
    {
        // Arrange
        Match match = NewStartedMatch();

        // Act
        Result result = match.Start();

        // Assert
        Assert.Equal(MatchErrors.NotScheduled, result.Error);
    }

    [Fact]
    public void ScoreGoal_MatchNotStarted_ReturnsNotInProgressError()
    {
        // Arrange
        Match match = NewMatch();

        // Act
        Result result = match.ScoreGoal(Side.Red);

        // Assert
        Assert.Equal(MatchErrors.NotInProgress, result.Error);
        Assert.Equal(0, match.RedScore);
    }

    [Fact]
    public void ScoreGoal_RedScores_IncrementsRedScoreAndRaisesGoalScored()
    {
        // Arrange
        Match match = NewStartedMatch();
        match.ClearDomainEvents();

        // Act
        Result result = match.ScoreGoal(Side.Red);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, match.RedScore);
        Assert.Equal(0, match.BlueScore);
        GoalScored goal = Assert.IsType<GoalScored>(Assert.Single(match.DomainEvents));
        Assert.Equal(Side.Red, goal.Side);
        Assert.Equal(1, goal.RedScore);
    }

    [Fact]
    public void ScoreGoal_TenthGoalOfBlueWithRedHavingScored_FinishesWithoutFanny()
    {
        // Arrange
        Match match = NewStartedMatch();
        match.ScoreGoal(Side.Red);
        match.ClearDomainEvents();

        // Act
        for (int goal = 0; goal < Match.WinningScore; goal++)
        {
            match.ScoreGoal(Side.Blue);
        }

        // Assert
        Assert.Equal(MatchStatus.Finished, match.Status);
        Assert.Equal(_blueId, match.WinnerId);
        MatchFinished finished = Assert.Single(match.DomainEvents.OfType<MatchFinished>());
        Assert.Equal(_blueId, finished.WinnerId);
        Assert.Equal(_redId, finished.LoserId);
        Assert.Empty(match.DomainEvents.OfType<FannyAchieved>());
    }

    [Fact]
    public void ScoreGoal_TenGoalsWhileTheOpponentNeverScored_RaisesFannyAchieved()
    {
        // Arrange
        Match match = NewStartedMatch();

        // Act
        for (int goal = 0; goal < Match.WinningScore; goal++)
        {
            match.ScoreGoal(Side.Red);
        }

        // Assert
        FannyAchieved fanny = Assert.Single(match.DomainEvents.OfType<FannyAchieved>());
        Assert.Equal(_redId, fanny.WinnerId);
        Assert.Equal(_blueId, fanny.LoserId);
        Assert.Single(match.DomainEvents.OfType<MatchFinished>());
    }

    [Fact]
    public void ScoreGoal_MatchFinished_ReturnsNotInProgressError()
    {
        // Arrange
        Match match = NewStartedMatch();

        for (int goal = 0; goal < Match.WinningScore; goal++)
        {
            match.ScoreGoal(Side.Red);
        }

        // Act
        Result result = match.ScoreGoal(Side.Blue);

        // Assert
        Assert.Equal(MatchErrors.NotInProgress, result.Error);
        Assert.Equal(0, match.BlueScore);
    }

    private Match NewMatch() => Match.Schedule(_redId, _blueId).Value;

    private Match NewStartedMatch()
    {
        Match match = NewMatch();
        match.Start();

        return match;
    }
}
