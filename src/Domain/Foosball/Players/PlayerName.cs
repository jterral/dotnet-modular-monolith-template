using Domain.Shared;

namespace Domain.Foosball.Players;

public sealed record PlayerName : IValueObject
{
    public const int MaxLength = 30;

    private PlayerName(string value) => Value = value;

    public string Value { get; }

    public static Result<PlayerName> Create(string? value)
    {
        string trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            return Result.Failure<PlayerName>(PlayerErrors.NameRequired);
        }

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<PlayerName>(PlayerErrors.NameTooLong);
        }

        return new PlayerName(trimmed);
    }

    public override string ToString() => Value;
}
