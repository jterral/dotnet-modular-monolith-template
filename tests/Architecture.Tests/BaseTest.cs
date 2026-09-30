using System.Reflection;
using Application.Shared.Messaging;
using ArchUnitNET.Loader;
using Domain.Shared;
using Infrastructure.Shared.Persistence;
using Arch = ArchUnitNET.Domain.Architecture;

namespace Architecture.Tests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(AggregateRoot).Assembly;

    protected static readonly Assembly ApplicationAssembly = typeof(ICommand).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;

    protected static readonly Arch Architecture = new ArchLoader()
        .LoadAssemblies(
            DomainAssembly,
            ApplicationAssembly,
            InfrastructureAssembly)
        .Build();
}
