using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Shop.Api.HealthChecks;

/// <summary>
/// Reports healthy when Redis answers a PING. The connection is created with
/// <c>AbortOnConnectFail = false</c>, so a down Redis shows up here rather than at startup.
/// </summary>
public sealed class RedisHealthCheck(IConnectionMultiplexer multiplexer) : IHealthCheck
{
    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(2);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!multiplexer.IsConnected)
            return HealthCheckResult.Unhealthy("Redis is not connected.");

        try
        {
            await multiplexer.GetDatabase().PingAsync().WaitAsync(PingTimeout, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis check failed.", exception);
        }
    }
}
