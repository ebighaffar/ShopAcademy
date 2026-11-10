using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shop.Api.Constants;

namespace Shop.Api.HealthChecks;

public static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";

    /// <summary>
    /// Registers the dependency checks used by the readiness probe. The liveness probe runs none.
    /// </summary>
    public static IServiceCollection AddShopHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres", HealthStatus.Unhealthy, [ReadyTag])
            .AddCheck<RedisHealthCheck>("redis", HealthStatus.Unhealthy, [ReadyTag])
            .AddCheck<MongoHealthCheck>("mongodb", HealthStatus.Unhealthy, [ReadyTag])
            // Degraded, not Unhealthy: a collector outage shows up in the status but stays HTTP 200.
            .AddCheck<OtlpCollectorHealthCheck>("otlp-collector", HealthStatus.Degraded, [ReadyTag]);

        return services;
    }

    /// <summary>
    /// Maps anonymous <c>/health/live</c> and <c>/health/ready</c>. Responses are the plain status
    /// text (Healthy / Degraded / Unhealthy); exception details are never written to the client.
    /// </summary>
    public static void MapHealthProbes(this WebApplication app)
    {
        app.MapHealthChecks(HealthUriConstants.Live, new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();

        app.MapHealthChecks(HealthUriConstants.Ready, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadyTag)
        }).AllowAnonymous();
    }
}
