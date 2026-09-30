using Application.Foosball;
using Application.Foosball.Matches;
using Application.Foosball.Matches.GetMatch;
using Application.Foosball.Matches.ScheduleMatch;
using Application.Foosball.Matches.ScoreGoal;
using Application.Foosball.Matches.StartMatch;
using Application.Foosball.Players.Leaderboard;
using Application.Foosball.Players.RegisterPlayer;
using Application.Shared;
using Application.Shared.Messaging;
using Domain.Foosball.Matches;
using Domain.Foosball.Players;
using Domain.Shared;
using Infrastructure.Foosball;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.DbTests.Foosball;

public sealed class FoosballModuleTests : IAsyncLifetime
{
    private const int SeededPlayers = 4;

    // A shared-cache in-memory database lives as long as one connection stays open, and the module opens
    // its own connections through the DbContext, so this one is only the keeper.
    private readonly string _connectionString = $"Data Source=foosball-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    private SqliteConnection _keeper = null!;
    private ServiceProvider _provider = null!;

    public async ValueTask InitializeAsync()
    {
        _keeper = new SqliteConnection(_connectionString);
        await _keeper.OpenAsync(TestContext.Current.CancellationToken);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{FoosballInfrastructureExtensions.ConnectionStringName}"] = _connectionString
            })
            .Build();

        _provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddFoosballApplication()
            .AddFoosballInfrastructure(configuration)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        await InitialiseModulesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _keeper.DisposeAsync();
    }

    [Fact]
    public async Task Initialise_CalledTwice_SeedsThePlayersOnlyOnce()
    {
        // Act
        await InitialiseModulesAsync();

        // Assert
        IReadOnlyList<LeaderboardEntry> entries = await LeaderboardAsync();
        Assert.Equal(SeededPlayers, entries.Count);
    }

    [Fact]
    public async Task RegisterPlayer_NameAlreadyTaken_ReturnsNameAlreadyTaken()
    {
        // Arrange
        await RegisterAsync("Henry");

        // Act
        Result<Guid> duplicate = await RegisterAsync("Henry");

        // Assert
        Assert.Equal(PlayerErrors.NameAlreadyTaken, duplicate.Error);
    }

    [Fact]
    public async Task ScheduleMatch_PlayerAlreadyInAnUnfinishedMatch_ReturnsPlayerBusy()
    {
        // Arrange
        Guid alice = (await RegisterAsync("Alice")).Value;
        Guid bob = (await RegisterAsync("Bob")).Value;
        Guid carol = (await RegisterAsync("Carol")).Value;
        await ScheduleAsync(alice, bob);

        // Act
        Result<Guid> result = await ScheduleAsync(alice, carol);

        // Assert
        Assert.Equal(MatchErrors.PlayerBusy, result.Error);
    }

    [Fact]
    public async Task ScoreGoal_TenthGoal_FinishesTheMatchAndUpdatesTheRatings()
    {
        // Arrange
        Guid alice = (await RegisterAsync("Alice")).Value;
        Guid bob = (await RegisterAsync("Bob")).Value;
        Guid matchId = (await ScheduleAsync(alice, bob)).Value;
        await StartAsync(matchId);

        // Act
        for (int goal = 0; goal < 3; goal++)
        {
            await ScoreAsync(matchId, Side.Blue);
        }

        for (int goal = 0; goal < Match.WinningScore; goal++)
        {
            await ScoreAsync(matchId, Side.Red);
        }

        // Assert
        MatchResponse match = (await GetMatchAsync(matchId)).Value;
        Assert.Equal(MatchStatus.Finished, match.Status);
        Assert.Equal(alice, match.WinnerId);
        Assert.Equal((Match.WinningScore, 3), (match.RedScore, match.BlueScore));

        IReadOnlyList<LeaderboardEntry> entries = await LeaderboardAsync();
        Assert.Equal(1016, entries.Single(entry => entry.PlayerId == alice).Rating);
        Assert.Equal(984, entries.Single(entry => entry.PlayerId == bob).Rating);
    }

    [Fact]
    public async Task ScoreGoal_AfterTheMatchIsFinished_ReturnsNotInProgress()
    {
        // Arrange
        Guid alice = (await RegisterAsync("Alice")).Value;
        Guid bob = (await RegisterAsync("Bob")).Value;
        Guid matchId = (await ScheduleAsync(alice, bob)).Value;
        await StartAsync(matchId);

        for (int goal = 0; goal < Match.WinningScore; goal++)
        {
            await ScoreAsync(matchId, Side.Red);
        }

        // Act
        Result result = await ScoreAsync(matchId, Side.Blue);

        // Assert
        Assert.Equal(MatchErrors.NotInProgress, result.Error);
    }

    private async Task InitialiseModulesAsync()
    {
        foreach (IModuleInitializer initializer in _provider.GetServices<IModuleInitializer>())
        {
            await initializer.InitialiseAsync(TestContext.Current.CancellationToken);
        }
    }

    // One scope per call, like a worker does: handlers and their DbContext never outlive a unit of work.
    private async Task<TResult> InScopeAsync<TResult>(Func<IServiceProvider, Task<TResult>> work)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();

        return await work(scope.ServiceProvider);
    }

    private Task<Result<Guid>> RegisterAsync(string name) =>
        InScopeAsync(services => services
            .GetRequiredService<ICommandHandler<RegisterPlayerCommand, Guid>>()
            .HandleAsync(new RegisterPlayerCommand(name), TestContext.Current.CancellationToken));

    private Task<Result<Guid>> ScheduleAsync(Guid redPlayerId, Guid bluePlayerId) =>
        InScopeAsync(services => services
            .GetRequiredService<ICommandHandler<ScheduleMatchCommand, Guid>>()
            .HandleAsync(new ScheduleMatchCommand(redPlayerId, bluePlayerId), TestContext.Current.CancellationToken));

    private Task<Result> StartAsync(Guid matchId) =>
        InScopeAsync(services => services
            .GetRequiredService<ICommandHandler<StartMatchCommand>>()
            .HandleAsync(new StartMatchCommand(matchId), TestContext.Current.CancellationToken));

    private Task<Result> ScoreAsync(Guid matchId, Side side) =>
        InScopeAsync(services => services
            .GetRequiredService<ICommandHandler<ScoreGoalCommand>>()
            .HandleAsync(new ScoreGoalCommand(matchId, side), TestContext.Current.CancellationToken));

    private Task<Result<MatchResponse>> GetMatchAsync(Guid matchId) =>
        InScopeAsync(services => services
            .GetRequiredService<IQueryHandler<GetMatchQuery, MatchResponse>>()
            .HandleAsync(new GetMatchQuery(matchId), TestContext.Current.CancellationToken));

    private async Task<IReadOnlyList<LeaderboardEntry>> LeaderboardAsync()
    {
        Result<IReadOnlyList<LeaderboardEntry>> result = await InScopeAsync(services => services
            .GetRequiredService<IQueryHandler<LeaderboardQuery, IReadOnlyList<LeaderboardEntry>>>()
            .HandleAsync(new LeaderboardQuery(100), TestContext.Current.CancellationToken));

        return result.Value;
    }
}
