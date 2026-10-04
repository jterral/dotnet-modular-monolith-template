namespace Application.Shared;

public interface IModuleInitializer
{
    string ModuleName { get; }

    Task InitialiseAsync(CancellationToken cancellationToken = default);
}
