using Microsoft.Data.Sqlite;

namespace Infrastructure.DbTests;

public sealed partial class SqliteTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void Test1()
    {
        // Arrange
        _connection.Open();

        // Act / Assert
        Assert.Equal(1, 1);
        Assert.Equal(2, 2);
    }
}
