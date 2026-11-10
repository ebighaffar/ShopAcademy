using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shop.Infrastructure.Persistence.Mongo;

namespace Shop.Api.HealthChecks;

/// <summary>
/// Reports healthy when MongoDB answers a <c>ping</c> command. The driver's own server-selection
/// timeout is 30 seconds, so the check enforces a shorter one.
/// </summary>
public sealed class MongoHealthCheck(MongoContext mongo) : IHealthCheck
{
    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(2);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(PingTimeout);

        try
        {
            await mongo.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: timeout.Token);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB check failed.", exception);
        }
    }
}
