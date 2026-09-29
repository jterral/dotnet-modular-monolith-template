using Application.Shared;
using Application.Shared.Events;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Shared.Persistence;

/// <summary>
/// Base commune : extraction et publication des domain events, rien d'autre.
/// Aucun DbSet ici — chaque contexte concret declare les siens et cible sa propre base
/// (EF Core resout les options par type de DbContext).
/// </summary>
public abstract class ApplicationDbContext(
    DbContextOptions options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : DbContext(options), IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Events publies APRES le SaveChanges : l'etat est persiste avant que les handlers tournent.
        // Les handlers partagent le scope, donc ce DbContext : ils peuvent reecrire dans la meme UoW.
        // Pour publier AVANT (meme transaction stricte), deplace l'appel au-dessus de base.SaveChangesAsync.
        var domainEvents = ExtractDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        await domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    private List<IDomainEvent> ExtractDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .ToList();

        // Materialiser AVANT de vider. AggregateRoot.DomainEvents expose une vue en lecture
        // seule sur la liste interne, pas une copie : vider dans une projection LINQ paresseuse
        // viderait aussi ce qu'on croit avoir deja collecte, et plus aucun event ne serait publie.
        var domainEvents = entities
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        foreach (AggregateRoot entity in entities)
        {
            entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
