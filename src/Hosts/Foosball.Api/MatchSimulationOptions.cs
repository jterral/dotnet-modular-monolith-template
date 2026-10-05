using System.ComponentModel.DataAnnotations;

namespace Foosball.Api;

internal sealed class MatchSimulationOptions
{
    public const string SectionName = "MatchSimulation";

    [Range(typeof(TimeSpan), "00:00:00.100", "01:00:00")]
    public TimeSpan TickInterval { get; init; } = TimeSpan.FromSeconds(1);
}
