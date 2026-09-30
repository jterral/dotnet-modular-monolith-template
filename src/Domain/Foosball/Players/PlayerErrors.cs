using Domain.Shared;

namespace Domain.Foosball.Players;

public static class PlayerErrors
{
    public static readonly Error NameRequired = new("Foosball.Player.NameRequired", "The player name is required.");

    public static readonly Error NameTooLong = new(
        "Foosball.Player.NameTooLong",
        $"The player name cannot exceed {PlayerName.MaxLength} characters.");

    public static readonly Error NameAlreadyTaken = new("Foosball.Player.NameAlreadyTaken", "This player name is already taken.");

    public static readonly Error NotFound = new("Foosball.Player.NotFound", "The player does not exist.");
}
