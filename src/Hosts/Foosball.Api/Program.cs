using System.Reflection;
using Application.Foosball;
using Foosball.Api.Jobs;
using Infrastructure.Foosball;
using Infrastructure.Shared.Api.Extensions;
using Infrastructure.Shared.HealthChecks;
using Infrastructure.Shared.Hosting;
using Infrastructure.Shared.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// ----------
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddFoosballApplication();
builder.Services.AddFoosballInfrastructure(builder.Configuration);

builder.Services.AddSerilogLogging(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddApiDocumentation();
builder.Services.AddGlobalExceptionHandler();

// The referee has no request scope: it resolves its handlers through ScopedExecutor.
builder.Services.AddScopedExecutor();
builder.Services
    .AddOptions<MatchSimulationOptions>()
    .Bind(builder.Configuration.GetSection(MatchSimulationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHostedService<MatchSimulationJob>();

// ----------
WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UsePathBase("/foosball");
app.UseRouting();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckTags.Ready) });

app.MapApiDocumentation();
app.MapEndpoints(app.MapApiVersion(1));

await app.RunAsync();
