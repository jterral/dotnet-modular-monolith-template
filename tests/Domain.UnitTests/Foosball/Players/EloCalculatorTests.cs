using Domain.Foosball.Players;

namespace Domain.UnitTests.Foosball.Players;

public sealed class EloCalculatorTests
{
    [Fact]
    public void Calculate_EqualRatings_WinnerGainsAndLoserLosesHalfTheKFactor()
    {
        // Act
        EloChange change = EloCalculator.Calculate(EloRating.Initial, EloRating.Initial);

        // Assert
        Assert.Equal(1016, change.Winner.Points);
        Assert.Equal(984, change.Loser.Points);
    }

    [Fact]
    public void Calculate_UnderdogWins_GainsMoreThanAFavouriteWould()
    {
        // Arrange
        EloRating underdog = EloRating.FromPoints(800);
        EloRating favourite = EloRating.FromPoints(1200);

        // Act
        EloChange underdogWins = EloCalculator.Calculate(underdog, favourite);
        EloChange favouriteWins = EloCalculator.Calculate(favourite, underdog);

        // Assert
        Assert.True(underdogWins.Winner.Points - 800 > favouriteWins.Winner.Points - 1200);
    }

    [Fact]
    public void Calculate_AnyResult_ConservesTheTotalOfPoints()
    {
        // Act
        EloChange change = EloCalculator.Calculate(EloRating.FromPoints(1100), EloRating.FromPoints(950));

        // Assert
        Assert.Equal(1100 + 950, change.Winner.Points + change.Loser.Points);
    }

    [Fact]
    public void FromPoints_NegativePoints_ClampsToZero()
    {
        // Act
        EloRating rating = EloRating.FromPoints(-5);

        // Assert
        Assert.Equal(0, rating.Points);
    }
}
