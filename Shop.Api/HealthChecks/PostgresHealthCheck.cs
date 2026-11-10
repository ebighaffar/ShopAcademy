using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shop.Infrastructure.Persistence.PostgreSql.EfCore;

namespace Shop.Api.HealthChecks;

/// <summary>
/// Reports healthy when the primary PostgreSQL database accepts a connection.
/// </summary>
public sealed class PostgresHealthCheck(ShopDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("PostgreSQL is unreachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL check failed.", exception);
        }
    }
}
