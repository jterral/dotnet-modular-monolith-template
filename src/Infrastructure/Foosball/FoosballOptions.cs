using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Foosball;

internal sealed class FoosballOptions
{
    public const string SectionName = "Foosball";

    // Deployments replace __PLACEHOLDERS__ in appsettings.json; one left behind would silently create a file named after it.
    [Required]
    [RegularExpression(@"^(?!.*__[A-Z0-9_]+__).*$", ErrorMessage = "The connection string still contains an unreplaced __PLACEHOLDER__.")]
    public string ConnectionString { get; init; } = string.Empty;
}
