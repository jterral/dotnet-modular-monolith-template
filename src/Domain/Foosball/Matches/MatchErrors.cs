using Domain.Shared;

namespace Domain.Foosball.Matches;

public static class MatchErrors
{
    public static readonly Error NotFound = new("Foosball.Match.NotFound", "The match does not exist.");

    public static readonly Error SamePlayer = new("Foosball.Match.SamePlayer", "A player cannot play against themselves.");

    public static readonly Error PlayerBusy = new("Foosball.Match.PlayerBusy", "A player is already in a match that is not finished.");

    public static readonly Error NotScheduled = new("Foosball.Match.NotScheduled", "Only a scheduled match can be started.");

    public static readonly Error NotInProgress = new("Foosball.Match.NotInProgress", "Goals can only be scored in a match in progress.");
}
