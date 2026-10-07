using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

public sealed class LayerDependencyTests : BaseTest
{
    private static readonly IObjectProvider<IType> _domainLayer =
        Types().That().ResideInAssembly(DomainAssembly).As("Domain layer");

    private static readonly IObjectProvider<IType> _applicationLayer =
        Types().That().ResideInAssembly(ApplicationAssembly).As("Application layer");

    private static readonly IObjectProvider<IType> _infrastructureLayer =
        Types().That().ResideInAssembly(InfrastructureAssembly).As("Infrastructure Layer");

    private static readonly IObjectProvider<IType> _infrastructureSharedApiLayer =
    Types().That().ResideInAssembly(InfrastructureSharedApiAssembly).As("Infrastructure.Shared.Api project");

    [Fact]
    public void Domain_Always_HasNoDependencyOnApplication()
    {
        Types().That().Are(_domainLayer).Should()
            .NotDependOnAny(_applicationLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Domain_Always_HasNoDependencyOnInfrastructure()
    {
        Types().That().Are(_domainLayer)
            .Should().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_infrastructureSharedApiLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Application_Always_HasNoDependencyOnInfrastructure()
    {
        Types().That().Are(_applicationLayer)
            .Should().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_infrastructureSharedApiLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Infrastructure_Always_HasNoDependencyOnInfrastructureSharedApi()
    {
        Types().That().Are(_infrastructureLayer).Should()
            .NotDependOnAny(_infrastructureSharedApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void CommandHandlers_Always_ResideInApplicationLayer()
    {
        Classes().That().HaveNameEndingWith("CommandHandler")
            .Should().ResideInAssembly(ApplicationAssembly)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_Always_ResideInApplicationLayer()
    {
        Classes().That().HaveNameEndingWith("QueryHandler")
            .Should().ResideInAssembly(ApplicationAssembly)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void InfrastructureSharedApi_Always_HasNoDependencyOnCoreLayersOrModules()
    {
        // Module code lives in the three core assemblies, so this also rules out every module folder.
        Types().That().Are(_infrastructureSharedApiLayer).Should()
            .NotDependOnAny(_domainLayer)
            .AndShould().NotDependOnAny(_applicationLayer)
            .AndShould().NotDependOnAny(_infrastructureLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
