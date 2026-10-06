using Asp.Versioning;
using Infrastructure.Shared.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Shared.Api.UnitTests;

public sealed class ApiServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApiDocumentation_WithoutVersions_DefaultsToVersionOne()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiDocumentation();

        // Assert
        Assert.Equal(new ApiVersion(1), GetVersioningOptions(services).DefaultApiVersion);
    }

    [Fact]
    public void AddApiDocumentation_WithSeveralVersions_UsesTheFirstAsDefault()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiDocumentation(2, 3);

        // Assert
        Assert.Equal(new ApiVersion(2), GetVersioningOptions(services).DefaultApiVersion);
    }

    [Fact]
    public void AddApiDocumentation_Always_ReadsTheVersionFromTheUrlSegment()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiDocumentation();

        // Assert
        Assert.IsType<UrlSegmentApiVersionReader>(GetVersioningOptions(services).ApiVersionReader);
    }

    private static ApiVersioningOptions GetVersioningOptions(IServiceCollection services)
    {
        using ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
    }
}
