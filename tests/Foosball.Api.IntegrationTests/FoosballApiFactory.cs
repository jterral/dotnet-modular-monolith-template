using Application.Shared;
using Foosball.Api.Jobs;
using Infrastructure.Shared.Api.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Foosball.Api.IntegrationTests;

/// <summary>
/// Runs the real host against its own throwaway SQLite file, without the referee.
/// </summary>
public sealed class FoosballApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"foosball-{Guid.NewGuid():N}.db");

    public async Task InitializeDatabaseAsync()
    {
        // The migrator is a separate host, so the tests play its part.
        foreach (IModuleInitializer initializer in Services.GetServices<IModuleInitializer>())
        {
            await initializer.InitialiseAsync();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        // SQLite keeps the file open until the connection pool is cleared, and the file is useless afterwards anyway.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // AddFoosballInfrastructure reads the connection string while Program runs, so it has to be a host setting:
        // a configuration source added later would be too late.
        builder.UseSetting("ConnectionStrings:Foosball", $"Data Source={_databasePath}");

        builder.ConfigureTestServices(services =>
        {
            // The referee starts a match on its first tick, which would make every expectation depend on timing.
            ServiceDescriptor referee = services.Single(descriptor =>
                descriptor.ServiceType == typeof(IHostedService) && descriptor.ImplementationType == typeof(MatchSimulationJob));
            services.Remove(referee);

            services.AddTransient<IEndpoint, ThrowingEndpoint>();
        });
    }
}
