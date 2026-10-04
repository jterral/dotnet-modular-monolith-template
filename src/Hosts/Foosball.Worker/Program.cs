using Application.Foosball;
using Foosball.Worker;
using Infrastructure.Foosball;
using Infrastructure.Shared.Hosting;
using Infrastructure.Shared.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilogLogging(builder.Configuration);
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
await host.RunAsync();
