using System.Xml.Linq;
using Testing.Shared.Extensions;

namespace Architecture.Tests;

// ArchUnitNET sees types, not project references, so this rule reads the .csproj files instead.
public sealed class ProjectReferenceTests
{
    private const string TechnologyProjectPrefix = "Infrastructure.Shared.";

    [Fact]
    public void InfrastructureSharedProjects_Always_AreReferencedOnlyByHosts()
    {
        // Arrange
        string sourceDirectory = Path.Combine(FindRepositoryRoot(), "src");
        string hostsDirectory = Path.Combine(sourceDirectory, "Hosts") + Path.DirectorySeparatorChar;

        // Act
        string[] offenders = [.. Directory
            .EnumerateFiles(sourceDirectory, "*.csproj", SearchOption.AllDirectories)
            .Where(project => !project.StartsWith(hostsDirectory, StringComparison.Ordinal))
            .Where(ReferencesTechnologyProject)
            .Select(project => Path.GetRelativePath(sourceDirectory, project))
            .Order()];

        // Assert
        AssertEx.Equal(
            string.Empty,
            string.Join(", ", offenders),
            $"Only hosts may reference an {TechnologyProjectPrefix}* project; the others pull its heavy dependencies into every host");
    }

    private static bool ReferencesTechnologyProject(string projectPath) => XDocument
        .Load(projectPath)
        .Descendants("ProjectReference")
        .Select(reference => (string?)reference.Attribute("Include"))
        .OfType<string>()
        .Select(include => Path.GetFileNameWithoutExtension(include.Replace('\\', '/')))
        .Any(name => name.StartsWith(TechnologyProjectPrefix, StringComparison.Ordinal));

    private static string FindRepositoryRoot()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModularMonolith.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("ModularMonolith.slnx not found above the test output directory.");
    }
}
