using Application.Foosball;
using Application.Shared;
using Foosball.Worker;
using Infrastructure.Foosball;
using Infrastructure.Shared.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ScopedExecutor>();
builder.Services.AddFoosballApplication();
builder.Services.AddFoosballInfrastructure(builder.Configuration);

builder.Services
    .AddOptions<MatchSimulationOptions>()
    .Bind(builder.Configuration.GetSection(MatchSimulationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHostedService<MatchSimulationWorker>();

using IHost host = builder.Build();

// Runs before the hosted services start, so the database exists when the first tick fires.
foreach (IModuleInitializer initializer in host.Services.GetServices<IModuleInitializer>())
{
    await initializer.InitialiseAsync();
}

await host.RunAsync();
