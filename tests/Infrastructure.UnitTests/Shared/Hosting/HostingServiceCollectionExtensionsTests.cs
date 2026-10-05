using Infrastructure.Shared.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.UnitTests.Shared.Hosting;

public sealed class HostingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddScopedExecutor_Always_RegistersItAsASingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddScopedExecutor();

        // Assert
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Same(
            provider.GetRequiredService<ScopedExecutor>(),
            provider.GetRequiredService<ScopedExecutor>());
    }

    [Fact]
    public void AddScopedExecutor_CalledTwice_RegistersItOnce()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddScopedExecutor().AddScopedExecutor();

        // Assert
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(ScopedExecutor));
    }
}
