using Application.Shared;

namespace Infrastructure.Shared.Time;

internal sealed class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
