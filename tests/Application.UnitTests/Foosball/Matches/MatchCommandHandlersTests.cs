using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Matches.ScoreGoal;
using Application.Foosball.Matches.StartMatch;
using Domain.Foosball.Matches;
using Domain.Shared;
using Moq;
using Match = Domain.Foosball.Matches.Match;

namespace Application.UnitTests.Foosball.Matches;

public sealed class MatchCommandHandlersTests : IDisposable
{
    private readonly Mock<IMatchRepository> _matchesMock = new();
    private readonly Mock<IFoosballUnitOfWork> _unitOfWorkMock = new();
    private readonly StartMatchCommandHandler _startHandler;
    private readonly ScoreGoalCommandHandler _scoreHandler;

    public MatchCommandHandlersTests()
    {
        _startHandler = new StartMatchCommandHandler(_matchesMock.Object, _unitOfWorkMock.Object);
        _scoreHandler = new ScoreGoalCommandHandler(_matchesMock.Object, _unitOfWorkMock.Object);
    }

    public void Dispose()
    {
        _matchesMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task StartMatch_ScheduledMatch_StartsItAndSaves()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Match match = NewMatch();
        _matchesMock.Setup(m => m.GetByIdAsync(match.Id, token)).ReturnsAsync(match);

        // Act
        Result result = await _startHandler.HandleAsync(new StartMatchCommand(match.Id), token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.InProgress, match.Status);
        _matchesMock.Verify(m => m.GetByIdAsync(match.Id, token), Times.Once());
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(token), Times.Once());
    }

    [Fact]
    public async Task StartMatch_UnknownMatch_ReturnsNotFound()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Guid id = Guid.CreateVersion7();
        _matchesMock.Setup(m => m.GetByIdAsync(id, token)).ReturnsAsync((Match?)null);

        // Act
        Result result = await _startHandler.HandleAsync(new StartMatchCommand(id), token);

        // Assert
        Assert.Equal(MatchErrors.NotFound, result.Error);
        _matchesMock.Verify(m => m.GetByIdAsync(id, token), Times.Once());
    }

    [Fact]
    public async Task ScoreGoal_MatchInProgress_ScoresAndSaves()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Match match = NewMatch();
        match.Start();
        _matchesMock.Setup(m => m.GetByIdAsync(match.Id, token)).ReturnsAsync(match);

        // Act
        Result result = await _scoreHandler.HandleAsync(new ScoreGoalCommand(match.Id, Side.Blue), token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, match.BlueScore);
        _matchesMock.Verify(m => m.GetByIdAsync(match.Id, token), Times.Once());
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(token), Times.Once());
    }

    [Fact]
    public async Task ScoreGoal_MatchNotStarted_ReturnsNotInProgressWithoutSaving()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Match match = NewMatch();
        _matchesMock.Setup(m => m.GetByIdAsync(match.Id, token)).ReturnsAsync(match);

        // Act
        Result result = await _scoreHandler.HandleAsync(new ScoreGoalCommand(match.Id, Side.Red), token);

        // Assert
        Assert.Equal(MatchErrors.NotInProgress, result.Error);
        _matchesMock.Verify(m => m.GetByIdAsync(match.Id, token), Times.Once());
    }

    private static Match NewMatch() => Match.Schedule(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;
}
