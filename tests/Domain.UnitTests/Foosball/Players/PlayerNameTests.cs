using Domain.Foosball.Players;
using Domain.Shared;

namespace Domain.UnitTests.Foosball.Players;

public sealed class PlayerNameTests
{
    [Fact]
    public void Create_ValidName_TrimsSurroundingWhitespace()
    {
        // Act
        Result<PlayerName> result = PlayerName.Create("  Zizou  ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Zizou", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_ReturnsNameRequiredError(string? name)
    {
        // Act
        Result<PlayerName> result = PlayerName.Create(name);

        // Assert
        Assert.Equal(PlayerErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Create_NameLongerThanTheMaximum_ReturnsNameTooLongError()
    {
        // Act
        Result<PlayerName> result = PlayerName.Create(new string('a', PlayerName.MaxLength + 1));

        // Assert
        Assert.Equal(PlayerErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Equals_SameValue_AreEqual()
    {
        // Act
        PlayerName first = PlayerName.Create("Kopa").Value;
        PlayerName second = PlayerName.Create("Kopa").Value;

        // Assert
        Assert.Equal(first, second);
    }
}
