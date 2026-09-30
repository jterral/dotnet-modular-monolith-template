using Application.Foosball;
using Application.Foosball.Matches.EventHandlers;
using Application.Foosball.Players;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Microsoft.Extensions.Logging;
using Moq;
using Testing.Shared.Extensions;

namespace Application.UnitTests.Foosball.Matches;

public sealed class UpdateRatingsOnMatchFinishedHandlerTests : IDisposable
{
    private readonly Mock<IPlayerRepository> _playersMock = new();
    private readonly Mock<IFoosballUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<UpdateRatingsOnMatchFinishedHandler>> _loggerMock = new();
    private readonly UpdateRatingsOnMatchFinishedHandler _sut;

    public UpdateRatingsOnMatchFinishedHandlerTests()
    {
        _sut = new UpdateRatingsOnMatchFinishedHandler(_playersMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _playersMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_BothPlayersExist_MovesTheRatingsAndSavesItself()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Player winner = Player.Create(PlayerName.Create("Zizou").Value);
        Player loser = Player.Create(PlayerName.Create("Platini").Value);
        _playersMock.Setup(p => p.GetByIdAsync(winner.Id, token)).ReturnsAsync(winner);
        _playersMock.Setup(p => p.GetByIdAsync(loser.Id, token)).ReturnsAsync(loser);
        var finished = new MatchFinished(Guid.CreateVersion7(), winner.Id, loser.Id, 10, 4);

        // Act
        await _sut.HandleAsync(finished, token);

        // Assert
        Assert.Equal(1016, winner.Rating.Points);
        Assert.Equal(984, loser.Rating.Points);
        _playersMock.Verify(p => p.GetByIdAsync(winner.Id, token), Times.Once());
        _playersMock.Verify(p => p.GetByIdAsync(loser.Id, token), Times.Once());
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(token), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once(), "Zizou", "Platini");
    }

    [Fact]
    public async Task HandleAsync_LoserNoLongerExists_LogsAWarningAndDoesNotSave()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        Player winner = Player.Create(PlayerName.Create("Zizou").Value);
        var finished = new MatchFinished(Guid.CreateVersion7(), winner.Id, Guid.CreateVersion7(), 10, 4);
        _playersMock.Setup(p => p.GetByIdAsync(winner.Id, token)).ReturnsAsync(winner);
        _playersMock.Setup(p => p.GetByIdAsync(finished.LoserId, token)).ReturnsAsync((Player?)null);

        // Act
        await _sut.HandleAsync(finished, token);

        // Assert
        Assert.Equal(EloRating.InitialPoints, winner.Rating.Points);
        _playersMock.Verify(p => p.GetByIdAsync(winner.Id, token), Times.Once());
        _playersMock.Verify(p => p.GetByIdAsync(finished.LoserId, token), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once(), finished.MatchId.ToString());
    }
}
