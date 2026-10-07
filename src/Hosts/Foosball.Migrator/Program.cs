using System.Runtime.InteropServices;
using Application.Shared;
using Infrastructure.Foosball;
using Infrastructure.Shared.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilogLogging(builder.Configuration);
builder.Services.AddFoosballInfrastructure(builder.Configuration);

// Disposing the host also disposes the Serilog logger, which flushes the Seq sink before the process exits.
using var host = builder.Build();
ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Foosball.Migrator");

// The host is never started, so its lifetime does not react to signals: Ctrl+C and `docker stop` (SIGTERM) are handled here.
using var cancellation = new CancellationTokenSource();
using PosixSignalRegistration sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, Cancel);
using PosixSignalRegistration sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Cancel);

try
{
    foreach (IModuleInitializer initializer in host.Services.GetServices<IModuleInitializer>())
    {
        await initializer.InitialiseAsync(cancellation.Token);
    }
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
    logger.LogWarning("Migration cancelled");

    return 130;
}
catch (Exception exception)
{
    // Logged here because an unhandled exception would only reach stderr, never Seq. The exit code keeps compose's `service_completed_successfully` honest.
    logger.LogCritical(exception, "Migration failed");

    return 1;
}

return 0;

void Cancel(PosixSignalContext context)
{
    context.Cancel = true;
    cancellation.Cancel();
}
