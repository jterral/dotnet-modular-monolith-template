using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Foosball;

internal sealed class FoosballOptions
{
    public const string SectionName = "Foosball";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}
