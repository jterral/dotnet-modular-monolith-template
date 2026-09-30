using Application.Foosball;
using Application.Foosball.Players;
using Application.Foosball.Players.RegisterPlayer;
using Domain.Foosball.Players;
using Domain.Shared;
using Moq;

namespace Application.UnitTests.Foosball.Players;

public sealed class RegisterPlayerCommandHandlerTests : IDisposable
{
    private readonly Mock<IPlayerRepository> _playersMock = new();
    private readonly Mock<IFoosballUnitOfWork> _unitOfWorkMock = new();
    private readonly RegisterPlayerCommandHandler _sut;

    public RegisterPlayerCommandHandlerTests()
    {
        _sut = new RegisterPlayerCommandHandler(_playersMock.Object, _unitOfWorkMock.Object);
    }

    public void Dispose()
    {
        _playersMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_NewName_AddsThePlayerAndSaves()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        _playersMock.Setup(p => p.NameExistsAsync(It.IsAny<PlayerName>(), token)).ReturnsAsync(false);
        Player? added = null;
        _playersMock.Setup(p => p.Add(It.IsAny<Player>())).Callback<Player>(player => added = player);

        // Act
        Result<Guid> result = await _sut.HandleAsync(new RegisterPlayerCommand("  Zizou "), token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(added.Id, result.Value);
        Assert.Equal("Zizou", added.Name.Value);
        _playersMock.Verify(p => p.NameExistsAsync(It.IsAny<PlayerName>(), token), Times.Once());
        _playersMock.Verify(p => p.Add(added), Times.Once());
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_NameAlreadyTaken_ReturnsNameAlreadyTakenWithoutSaving()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        _playersMock.Setup(p => p.NameExistsAsync(It.IsAny<PlayerName>(), token)).ReturnsAsync(true);

        // Act
        Result<Guid> result = await _sut.HandleAsync(new RegisterPlayerCommand("Zizou"), token);

        // Assert
        Assert.Equal(PlayerErrors.NameAlreadyTaken, result.Error);
        _playersMock.Verify(p => p.NameExistsAsync(It.IsAny<PlayerName>(), token), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_BlankName_ReturnsNameRequiredWithoutTouchingTheRepository()
    {
        // Act
        Result<Guid> result = await _sut.HandleAsync(new RegisterPlayerCommand(" "), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(PlayerErrors.NameRequired, result.Error);
    }
}
