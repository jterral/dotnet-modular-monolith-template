using Application.Shared.Messaging;
using ArchUnitNET.xUnitV3;
using Domain.Shared;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

public sealed class VisibilityTests : BaseTest
{
    [Fact]
    public void CommandHandlers_Always_BeInternal()
    {
        Classes().That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should().BeInternal()
            .Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_ShouldBeInternal()
    {
        Classes().That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should().BeInternal()
            .Check(Architecture);
    }

    [Fact(Skip = "No IValueObject implementations yet")]
    public void DomainValueObjects_ShouldBeRecord()
    {
        Classes().That().ImplementInterface(typeof(IValueObject))
            .Should()
            .BeRecord()
            .AndShould()
            .BeSealed()
            .Check(Architecture);
    }

    [Fact]
    public void DomainEntities_ShouldNotBeRecord()
    {
        Classes().That().AreAssignableTo(typeof(Entity)).Should()
            .NotBeRecord()
            .Check(Architecture);
    }
}
