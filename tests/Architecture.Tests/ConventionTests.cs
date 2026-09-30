using Application.Shared.Messaging;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

public sealed class NamingConventionTests : BaseTest
{
    [Fact]
    public void CommandHandlers_ShouldHaveCommandHandlerSuffix()
    {
        Classes().That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And().DoNotResideInNamespace("Application.Shared.Behaviors")
            .Should().HaveNameEndingWith("CommandHandler")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_ShouldHave_NameEndingWith_QueryHandler()
    {
        Classes().That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .And().DoNotResideInNamespace("Application.Shared.Behaviors")
            .Should().HaveNameEndingWith("QueryHandler")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void Validators_ShouldHave_NameEndingWith_Validator()
    {
        Classes().That()
            .HaveNameEndingWith("Validator")
            .Should().ResideInAssembly(ApplicationAssembly)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
