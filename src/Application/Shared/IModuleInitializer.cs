namespace Application.Shared;

/// <summary>
/// Preparation au demarrage d'un module (base de donnees, caches, warm-up).
/// Chaque module enregistre SON implementation ; l'hote en appelle une seule ligne qui
/// resout IEnumerable&lt;IModuleInitializer&gt;. Ce n'est pas du scan d'assembly : les
/// enregistrements restent explicites, c'est la DI qui agrege.
/// </summary>
public interface IModuleInitializer
{
    string ModuleName { get; }

    Task InitialiseAsync(CancellationToken cancellationToken = default);
}
