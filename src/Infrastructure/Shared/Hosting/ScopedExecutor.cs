using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared.Hosting;

public sealed class ScopedExecutor(IServiceScopeFactory scopeFactory)
{
    public async Task ExecuteAsync<TService>(
        Func<TService, CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
        where TService : notnull
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        TService service = scope.ServiceProvider.GetRequiredService<TService>();

        await work(service, cancellationToken);
    }
}
