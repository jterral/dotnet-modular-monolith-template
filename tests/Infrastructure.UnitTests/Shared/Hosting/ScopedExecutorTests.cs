using Infrastructure.Shared.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.UnitTests.Shared.Hosting;

public sealed class ScopedExecutorTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly ScopedExecutor _sut;

    public ScopedExecutorTests()
    {
        _provider = new ServiceCollection().AddScoped<ScopeProbe>().BuildServiceProvider();
        _sut = new ScopedExecutor(_provider.GetRequiredService<IServiceScopeFactory>());
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Fact]
    public async Task ExecuteAsync_Work_ReceivesTheServiceAndTheCancellationToken()
    {
        // Arrange
        CancellationToken token = TestContext.Current.CancellationToken;
        ScopeProbe? received = null;
        CancellationToken receivedToken = default;

        // Act
        await _sut.ExecuteAsync<ScopeProbe>(
            (probe, cancellationToken) =>
            {
                received = probe;
                receivedToken = cancellationToken;
                return Task.CompletedTask;
            },
            token);

        // Assert
        Assert.NotNull(received);
        Assert.Equal(token, receivedToken);
    }

    [Fact]
    public async Task ExecuteAsync_WorkReturningAValue_ReturnsThatValue()
    {
        // Act
        int result = await _sut.ExecuteAsync<ScopeProbe, int>(
            (_, _) => Task.FromResult(42),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_WorkReturningAValue_ReceivesTheServiceAndDisposesTheScopeAfterwards()
    {
        // Arrange
        ScopeProbe? received = null;
        bool disposedDuringWork = true;

        // Act
        await _sut.ExecuteAsync<ScopeProbe, int>(
            (probe, _) =>
            {
                received = probe;
                disposedDuringWork = probe.IsDisposed;
                return Task.FromResult(42);
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(received);
        Assert.False(disposedDuringWork);
        Assert.True(received.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_WorkReturningAValueThrows_DisposesTheScopeAndPropagatesTheException()
    {
        // Arrange
        ScopeProbe? received = null;
        var failure = new InvalidOperationException("The work failed.");

        // Act
        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ExecuteAsync<ScopeProbe, int>(
                (probe, _) =>
                {
                    received = probe;
                    return Task.FromException<int>(failure);
                },
                TestContext.Current.CancellationToken));

        // Assert
        Assert.Same(failure, thrown);
        Assert.NotNull(received);
        Assert.True(received.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_Work_RunsInAScopeThatIsDisposedAfterwards()
    {
        // Arrange
        ScopeProbe? received = null;
        bool disposedDuringWork = true;

        // Act
        await _sut.ExecuteAsync<ScopeProbe>(
            (probe, _) =>
            {
                received = probe;
                disposedDuringWork = probe.IsDisposed;
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(received);
        Assert.False(disposedDuringWork);
        Assert.True(received.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_TwoCalls_ResolveDistinctInstances()
    {
        // Arrange
        ScopeProbe? first = null;
        ScopeProbe? second = null;

        // Act
        await _sut.ExecuteAsync<ScopeProbe>(
            (probe, _) =>
            {
                first = probe;
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);
        await _sut.ExecuteAsync<ScopeProbe>(
            (probe, _) =>
            {
                second = probe;
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
        ScopeProbe? received = null;
        var failure = new InvalidOperationException("The work failed.");

        // Act
        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ExecuteAsync<ScopeProbe>(
                (probe, _) =>
                {
                    received = probe;
                    return Task.FromException(failure);
                },
                TestContext.Current.CancellationToken));

        // Assert
        Assert.Same(failure, thrown);
        Assert.NotNull(received);
        Assert.True(received.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotRegistered_Throws()
    {
        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ExecuteAsync<UnregisteredService>(
                (_, _) => Task.CompletedTask,
                TestContext.Current.CancellationToken));
    }

    private sealed class ScopeProbe : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class UnregisteredService;
}
