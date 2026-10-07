using Infrastructure.Shared.Api;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared.Api.UnitTests;

public sealed class EndpointExtensionsTests
{
    [Fact]
    public void AddEndpoints_Always_RegistersConcreteEndpointsOfTheAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpoints(typeof(EndpointExtensionsTests).Assembly);

        // Assert
        ServiceDescriptor descriptor = Assert.Single(services, d => d.ServiceType == typeof(IEndpoint));
        Assert.Equal(typeof(SampleEndpoint), descriptor.ImplementationType);
    }

    [Fact]
    public void AddEndpoints_CalledTwice_RegistersEachEndpointOnce()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services
            .AddEndpoints(typeof(EndpointExtensionsTests).Assembly)
            .AddEndpoints(typeof(EndpointExtensionsTests).Assembly);

        // Assert
        Assert.Single(services, d => d.ServiceType == typeof(IEndpoint));
    }

    private sealed class SampleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
        }
    }

    private abstract class AbstractEndpoint : IEndpoint
    {
        public abstract void MapEndpoint(IEndpointRouteBuilder app);
    }
}
