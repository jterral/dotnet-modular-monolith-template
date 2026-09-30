namespace Application.Shared;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
