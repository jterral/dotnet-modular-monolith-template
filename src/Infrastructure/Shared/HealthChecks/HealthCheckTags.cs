namespace Infrastructure.Shared.HealthChecks;

public static class HealthCheckTags
{
    // Only checks tagged Ready run on /health/ready; /health/live runs none, it just proves the process answers.
    public const string Ready = "ready";
}
