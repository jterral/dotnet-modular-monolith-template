using Infrastructure.Foosball;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.Foosball;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddFoosballInfrastructure_MissingConnectionString_FailsOptionsValidation()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder().Build();
        using ServiceProvider provider = new ServiceCollection()
            .AddFoosballInfrastructure(configuration)
            .BuildServiceProvider();

        // Act
        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<FoosballOptions>>().Value);

        // Assert
        Assert.Contains("ConnectionString", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddFoosballInfrastructure_UnreplacedPlaceholder_FailsOptionsValidation()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Foosball:ConnectionString"] = "Data Source=__SQLITE_DB_PATH__" })
            .Build();
        using ServiceProvider provider = new ServiceCollection()
            .AddFoosballInfrastructure(configuration)
            .BuildServiceProvider();

        // Act
        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<FoosballOptions>>().Value);

        // Assert
        Assert.Contains("PLACEHOLDER", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddFoosballInfrastructure_ConnectionStringProvided_BindsTheOptions()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Foosball:ConnectionString"] = "Data Source=test.db" })
            .Build();
        using ServiceProvider provider = new ServiceCollection()
            .AddFoosballInfrastructure(configuration)
            .BuildServiceProvider();

        // Act
        FoosballOptions options = provider.GetRequiredService<IOptions<FoosballOptions>>().Value;

        // Assert
        Assert.Equal("Data Source=test.db", options.ConnectionString);
    }
}
