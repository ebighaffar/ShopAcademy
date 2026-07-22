using System.Text.Json;
using Microsoft.Extensions.Options;
using Shop.Application.Caching;
using StackExchange.Redis;

namespace Shop.Infrastructure.Caching.Redis;

/// <summary>
/// Redis-backed <see cref="ICacheService"/>. Values are stored as JSON strings; keys are
/// namespaced with <see cref="RedisSettings.InstanceName"/> so this app's entries don't
/// collide with others sharing the same Redis instance.
/// </summary>
public class RedisCacheService(RedisConnection connection, IOptions<RedisSettings> settings) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RedisSettings _settings = settings.Value;
    private IDatabase Db => connection.Database;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetAsync(BuildKey(key));
        return value.IsNullOrEmpty
            ? default
            : JsonSerializer.Deserialize<T>(value.ToString(), SerializerOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value, SerializerOptions);
        return Db.StringSetAsync(BuildKey(key), payload, expiration ?? DefaultExpiration);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Db.KeyDeleteAsync(BuildKey(key));

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        => Db.KeyExistsAsync(BuildKey(key));

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var value = await factory(cancellationToken);
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    private TimeSpan DefaultExpiration => TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

    private string BuildKey(string key) => $"{_settings.InstanceName}{key}";
}
