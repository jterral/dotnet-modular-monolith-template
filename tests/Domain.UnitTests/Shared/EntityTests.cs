using Domain.Shared;

namespace Domain.UnitTests.Shared;

public sealed class EntityTests
{
    [Fact]
    public void Id_NewEntity_IsNotEmpty()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Id_TwoEntities_AreDistinct()
    {
        // Act
        var first = new TestEntity();
        var second = new TestEntity();

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Id_NewEntity_IsAVersion7Guid()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        Assert.Equal(7, entity.Id.Version);
    }

    private sealed class TestEntity : Entity;
}
