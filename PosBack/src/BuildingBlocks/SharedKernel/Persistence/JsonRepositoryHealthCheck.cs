using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SharedKernel.Persistence;

public sealed class JsonRepositoryHealthCheck : IHealthCheck
{
    private readonly JsonRepositoryOptions _options;

    public JsonRepositoryHealthCheck(JsonRepositoryOptions options)
    {
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetFullPath(_options.DataPath);
        return Task.FromResult(Directory.Exists(directory)
            ? HealthCheckResult.Healthy($"JSON data directory available: {directory}")
            : HealthCheckResult.Unhealthy($"JSON data directory missing: {directory}"));
    }
}
