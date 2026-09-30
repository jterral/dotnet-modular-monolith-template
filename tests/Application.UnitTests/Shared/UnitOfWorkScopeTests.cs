using Application.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Shared;

public sealed class UnitOfWorkScopeTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly UnitOfWorkScope _sut;

    public UnitOfWorkScopeTests()
    {
        _provider = new ServiceCollection().AddScoped<ScopeProbe>().BuildServiceProvider();
        _sut = new UnitOfWorkScope(_provider.GetRequiredService<IServiceScopeFactory>());
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task ExecuteAsync_WorkReturningAValue_ReturnsThatValue()
    {
        // Act
        int result = await _sut.ExecuteAsync((_, _) => Task.FromResult(42), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_Work_ReceivesTheCancellationToken()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        CancellationToken received = default;

        // Act
        await _sut.ExecuteAsync(
            (_, cancellationToken) =>
            {
                received = cancellationToken;
                return Task.CompletedTask;
            },
            token);

        // Assert
        Assert.Equal(token, received);
    }

    [Fact]
    public async Task ExecuteAsync_Work_RunsInAScopeThatIsDisposedAfterwards()
    {
        // Arrange
        ScopeProbe? probe = null;
        bool disposedDuringWork = true;

        // Act
        await _sut.ExecuteAsync(
            (serviceProvider, _) =>
            {
                probe = serviceProvider.GetRequiredService<ScopeProbe>();
                disposedDuringWork = probe.IsDisposed;
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(probe);
        Assert.False(disposedDuringWork);
        Assert.True(probe.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_TwoCalls_UseDistinctScopes()
    {
        // Arrange
        ScopeProbe? first = null;
        ScopeProbe? second = null;

        // Act
        await _sut.ExecuteAsync(
            (serviceProvider, _) =>
            {
                first = serviceProvider.GetRequiredService<ScopeProbe>();
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);
        await _sut.ExecuteAsync(
            (serviceProvider, _) =>
            {
                second = serviceProvider.GetRequiredService<ScopeProbe>();
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task ExecuteAsync_WorkThrows_DisposesTheScopeAndPropagatesTheException()
    {
        // Arrange
        ScopeProbe? probe = null;
        var failure = new InvalidOperationException("The work failed.");

        // Act
        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ExecuteAsync(
                (serviceProvider, _) =>
                {
                    probe = serviceProvider.GetRequiredService<ScopeProbe>();
                    return Task.FromException(failure);
                },
                TestContext.Current.CancellationToken));

        // Assert
        Assert.Same(failure, thrown);
        Assert.NotNull(probe);
        Assert.True(probe.IsDisposed);
    }

    private sealed class ScopeProbe : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
