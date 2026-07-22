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
    private readonly ConnectionMultiplexer
        _multiplexer = ConnectionMultiplexer.Connect(settings.Value.ConnectionString);

    public IDatabase Database => _multiplexer.GetDatabase();

    public void Dispose() => _multiplexer.Dispose();
}