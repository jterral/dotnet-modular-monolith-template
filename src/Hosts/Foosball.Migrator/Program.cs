using Application.Shared;
using Infrastructure.Foosball;
using Infrastructure.Shared.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilogLogging(builder.Configuration);
builder.Services.AddFoosballInfrastructure(builder.Configuration);

using var host = builder.Build();

foreach (IModuleInitializer initializer in host.Services.GetServices<IModuleInitializer>())
{
    await initializer.InitialiseAsync();
}
