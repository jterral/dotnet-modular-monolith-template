using Infrastructure.Foosball;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.UnitTests.Foosball;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddFoosballInfrastructure_MissingConnectionString_Throws()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddFoosballInfrastructure(configuration));

        // Assert
        Assert.Contains("missing", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddFoosballInfrastructure_UnreplacedPlaceholder_Throws()
    {
        // Arrange
        IConfiguration configuration = WithConnectionString("__SQLITE_DB_PATH__");

        // Act
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddFoosballInfrastructure(configuration));

        // Assert
        Assert.Contains("PLACEHOLDER", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddFoosballInfrastructure_ConnectionStringProvided_DoesNotThrow()
    {
        // Arrange
        IConfiguration configuration = WithConnectionString("Data Source=test.db");

        // Act
        IServiceCollection services = new ServiceCollection().AddFoosballInfrastructure(configuration);

        // Assert
        Assert.NotEmpty(services);
    }

    private static IConfiguration WithConnectionString(string connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"ConnectionStrings:{DependencyInjection.ConnectionStringName}"] = connectionString })
            .Build();
}
