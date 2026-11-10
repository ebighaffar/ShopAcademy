using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Shop.Infrastructure.Caching.Redis;

/// <summary>
/// Owns the Redis <see cref="ConnectionMultiplexer"/> and exposes an <see cref="IDatabase"/>.
/// Registered as a singleton — the multiplexer is expensive to create, thread-safe, and
/// designed to be shared across the whole application (mirrors how MongoContext holds the
/// Mongo client). Implements <see cref="IDisposable"/> so the connection is closed on shutdown.
/// </summary>
public sealed class RedisConnection(IOptions<RedisSettings> settings) : IDisposable
{
    private readonly ConnectionMultiplexer _multiplexer = Connect(settings.Value.ConnectionString);

    public IConnectionMultiplexer Multiplexer => _multiplexer;

    public IDatabase Database => _multiplexer.GetDatabase();

    public void Dispose() => _multiplexer.Dispose();

    // OpenTelemetry's Redis instrumentation resolves this connection while the app starts. Keep retrying
    // in the background if Redis is down, so an outage fails cache calls rather than the whole startup.
    private static ConnectionMultiplexer Connect(string connectionString)
    {
        var options = ConfigurationOptions.Parse(connectionString);
        options.AbortOnConnectFail = false;

        return ConnectionMultiplexer.Connect(options);
    }
}