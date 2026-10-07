using System.Net;
using System.Net.Http.Json;
using Application.Foosball.Matches.ScheduleMatch;
using Application.Foosball.Matches.StartMatch;
using Application.Foosball.Players.Leaderboard;
using Application.Shared.Messaging;
using Domain.Shared;
using Foosball.Api.Endpoints;
using Microsoft.Extensions.DependencyInjection;

namespace Foosball.Api.IntegrationTests;

public sealed class FoosballApiTests : IAsyncLifetime
{
    private const string OverviewPath = "/foosball/api/v1/foosball";

    // A new instance per test: every test gets its own host and its own database.
    private readonly FoosballApiFactory _factory = new();

    public async ValueTask InitializeAsync() => await _factory.InitializeDatabaseAsync();

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task GetOverview_NoMatchInProgress_ReturnsThePlayersRankedByRating()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(OverviewPath, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        FoosballOverview? overview = await response.Content.ReadFromJsonAsync<FoosballOverview>(TestContext.Current.CancellationToken);
        Assert.NotNull(overview);
        Assert.Equal("players", overview.Kind);
        Assert.Empty(overview.Matches);
        Assert.Equal(4, overview.Players.Count);
        Assert.Equal([1, 2, 3, 4], overview.Players.Select(player => player.Rank));
        Assert.Equal(overview.Players.OrderByDescending(player => player.Rating), overview.Players);
    }

    [Fact]
    public async Task GetOverview_MatchInProgress_ReturnsTheMatches()
    {
        // Arrange
        Guid matchId = await StartMatchAsync();
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(OverviewPath, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        FoosballOverview? overview = await response.Content.ReadFromJsonAsync<FoosballOverview>(TestContext.Current.CancellationToken);
        Assert.NotNull(overview);
        Assert.Equal("matches", overview.Kind);
        Assert.Empty(overview.Players);
        Assert.Equal(matchId, Assert.Single(overview.Matches).Id);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/foosball/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/foosball/health/ready")]
    public async Task GetHealth_MigratedDatabase_ReturnsOk(string path)
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOpenApiDocument_Always_DescribesTheVersionedRoute()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/foosball/openapi/v1.json", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string document = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("/api/v1/foosball", document, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetRoute_UnhandledException_ReturnsProblemDetailsWithoutTheExceptionMessage()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/foosball/api/v1{ThrowingEndpoint.Path}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("An unexpected error occurred.", body, StringComparison.Ordinal);
        Assert.DoesNotContain(ThrowingEndpoint.Secret, body, StringComparison.Ordinal);
    }

    private async Task<Guid> StartMatchAsync()
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        Result<IReadOnlyList<LeaderboardEntry>> players = await services
            .GetRequiredService<IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>>>()
            .HandleAsync(new LeaderboardQuery(), cancellationToken);

        Result<Guid> scheduled = await services
            .GetRequiredService<ICommandHandler<ScheduleMatchCommand, Guid>>()
            .HandleAsync(new ScheduleMatchCommand(players.Value[0].PlayerId, players.Value[1].PlayerId), cancellationToken);

        Result started = await services
            .GetRequiredService<ICommandHandler<StartMatchCommand>>()
            .HandleAsync(new StartMatchCommand(scheduled.Value), cancellationToken);

        Assert.True(started.IsSuccess, started.Error.Description);

        return scheduled.Value;
    }
}
