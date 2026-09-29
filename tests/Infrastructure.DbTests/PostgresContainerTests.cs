using Testcontainers.PostgreSql;
using Testcontainers.Xunit;

namespace Infrastructure.DbTests;

public sealed partial class PostgresContainerTests(ITestOutputHelper testOutputHelper)
    : ContainerTest<PostgreSqlBuilder, PostgreSqlContainer>(testOutputHelper)
{
    protected override PostgreSqlBuilder Configure() => new("postgres:18.6");

    [Fact]
    public void Test1()
    {
        // Arrange / Act / Assert
        Assert.Equal(1, 1);
        Assert.Equal(2, 2);
    }
}
