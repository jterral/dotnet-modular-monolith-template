using Application.Foosball.Matches.ScheduleMatch;
using Domain.Shared;

namespace Application.UnitTests.Foosball.Matches;

public sealed class ScheduleMatchCommandValidatorTests
{
    private readonly ScheduleMatchCommandValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_BothIdsProvided_Succeeds()
    {
        // Act
        Result result = await _sut.ValidateAsync(
            new ScheduleMatchCommand(Guid.CreateVersion7(), Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_EmptyPlayerId_Fails()
    {
        // Act
        Result result = await _sut.ValidateAsync(
            new ScheduleMatchCommand(Guid.Empty, Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }
}
