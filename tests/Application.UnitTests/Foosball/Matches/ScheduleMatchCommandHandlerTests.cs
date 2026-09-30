using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Matches.ScheduleMatch;
using Application.Foosball.Players;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Domain.Shared;
using Moq;
using Match = Domain.Foosball.Matches.Match;

namespace Application.UnitTests.Foosball.Matches;

public sealed class ScheduleMatchCommandHandlerTests : IDisposable
{
    private readonly Mock<IPlayerRepository> _playersMock = new();
    private readonly Mock<IMatchRepository> _matchesMock = new();
    private readonly Mock<IFoosballUnitOfWork> _unitOfWorkMock = new();
    private readonly ScheduleMatchCommandHandler _sut;

    public ScheduleMatchCommandHandlerTests()
    {
        _sut = new ScheduleMatchCommandHandler(_playersMock.Object, _matchesMock.Object, _unitOfWorkMock.Object);
    }

    public void Dispose()
    {
        _playersMock.VerifyNoOtherCalls();
        _matchesMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_TwoFreePlayers_SchedulesTheMatchAndSaves()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Player red = NewPlayer("Zizou");
        Player blue = NewPlayer("Platini");
        _playersMock.Setup(p => p.GetByIdAsync(red.Id, token)).ReturnsAsync(red);
        _playersMock.Setup(p => p.GetByIdAsync(blue.Id, token)).ReturnsAsync(blue);
        _matchesMock.Setup(m => m.HasUnfinishedMatchAsync(It.IsAny<Guid>(), token)).ReturnsAsync(false);
        Match? added = null;
        _matchesMock.Setup(m => m.Add(It.IsAny<Match>())).Callback<Match>(match => added = match);

        // Act
        Result<Guid> result = await _sut.HandleAsync(new ScheduleMatchCommand(red.Id, blue.Id), token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(added.Id, result.Value);
        Assert.Equal(red.Id, added.RedPlayerId);
        Assert.Equal(blue.Id, added.BluePlayerId);
        _playersMock.Verify(p => p.GetByIdAsync(red.Id, token), Times.Once());
        _playersMock.Verify(p => p.GetByIdAsync(blue.Id, token), Times.Once());
        _matchesMock.Verify(m => m.HasUnfinishedMatchAsync(red.Id, token), Times.Once());
        _matchesMock.Verify(m => m.HasUnfinishedMatchAsync(blue.Id, token), Times.Once());
        _matchesMock.Verify(m => m.Add(added), Times.Once());
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_SamePlayerOnBothSides_ReturnsSamePlayerError()
    {
        // Arrange
        Guid id = Guid.CreateVersion7();

        // Act
        Result<Guid> result = await _sut.HandleAsync(new ScheduleMatchCommand(id, id), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(MatchErrors.SamePlayer, result.Error);
    }

    [Fact]
    public async Task HandleAsync_UnknownPlayer_ReturnsPlayerNotFound()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        var command = new ScheduleMatchCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        _playersMock.Setup(p => p.GetByIdAsync(command.RedPlayerId, token)).ReturnsAsync((Player?)null);

        // Act
        Result<Guid> result = await _sut.HandleAsync(command, token);

        // Assert
        Assert.Equal(PlayerErrors.NotFound, result.Error);
        _playersMock.Verify(p => p.GetByIdAsync(command.RedPlayerId, token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_PlayerAlreadyInAMatch_ReturnsPlayerBusy()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Player red = NewPlayer("Zizou");
        _playersMock.Setup(p => p.GetByIdAsync(red.Id, token)).ReturnsAsync(red);
        _matchesMock.Setup(m => m.HasUnfinishedMatchAsync(red.Id, token)).ReturnsAsync(true);

        // Act
        Result<Guid> result = await _sut.HandleAsync(new ScheduleMatchCommand(red.Id, Guid.CreateVersion7()), token);

        // Assert
        Assert.Equal(MatchErrors.PlayerBusy, result.Error);
        _playersMock.Verify(p => p.GetByIdAsync(red.Id, token), Times.Once());
        _matchesMock.Verify(m => m.HasUnfinishedMatchAsync(red.Id, token), Times.Once());
    }

    private static Player NewPlayer(string name) => Player.Create(PlayerName.Create(name).Value);
}
