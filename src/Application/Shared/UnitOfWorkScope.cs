using Microsoft.Extensions.DependencyInjection;

namespace Application.Shared;

/// <summary>
/// Point d'entree d'une unite de travail pour un hote SANS scope ambiant
/// (worker, consumer, CLI). ASP.NET Core en cree un par requete HTTP ; un BackgroundService non.
/// MediatR n'a jamais fourni ce scope — c'est l'hote qui le fait.
///
/// NE PAS utiliser depuis ASP.NET Core : CreateAsyncScope() cree un scope FRERE de celui de la
/// requete, pas un enfant. On se retrouve avec deux DbContext, et l'ecriture faite via les
/// dependances injectees dans l'endpoint n'est jamais sauvegardee.
///
/// Singleton : ne depend que de IServiceScopeFactory, jamais d'un DbContext.
/// </summary>
public sealed class UnitOfWorkScope(IServiceScopeFactory scopeFactory)
{
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<IServiceProvider, CancellationToken, Task<TResult>> work,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        return await work(scope.ServiceProvider, cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<IServiceProvider, CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        await work(scope.ServiceProvider, cancellationToken);
    }
}
