namespace Shop.Application.Caching;

/// <summary>
/// Abstraction over a distributed cache. The Application layer depends only on this
/// contract; the concrete backing store (Redis) lives in the Infrastructure layer.
/// Values are serialized by the implementation, so callers work with plain objects.
/// </summary>
public interface ICacheService
{
    /// <summary>Returns the cached value for <paramref name="key"/>, or null if absent or expired.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores <paramref name="value"/> under <paramref name="key"/>, optionally expiring after <paramref name="expiration"/>.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>Removes the entry for <paramref name="key"/> if present.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>True if an entry exists for <paramref name="key"/>.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache-aside helper: returns the cached value, or invokes <paramref name="factory"/>,
    /// caches its result under <paramref name="key"/>, and returns it.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
}
