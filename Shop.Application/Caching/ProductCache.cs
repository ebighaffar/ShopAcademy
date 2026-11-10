using Shop.Application.Caching.Constants;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Telemetry;

namespace Shop.Application.Caching;

public sealed class ProductCache(ICacheService cacheService)
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public Task<IReadOnlyList<ProductDto>> GetAllAsync(
        Func<CancellationToken, Task<IReadOnlyList<ProductDto>>> factory,
        CancellationToken cancellationToken = default)
        => GetOrSetAsync(ProductCacheKeyConstants.All, factory, cancellationToken);

    public Task<ProductDto> GetByIdAsync(
        long id,
        Func<CancellationToken, Task<ProductDto>> factory,
        CancellationToken cancellationToken = default)
        => GetOrSetAsync(ProductCacheKeyConstants.ById(id), factory, cancellationToken);

    public async Task InvalidateAsync(long id, CancellationToken cancellationToken = default)
    {
        await cacheService.RemoveAsync(ProductCacheKeyConstants.ById(id), cancellationToken);
        await cacheService.RemoveAsync(ProductCacheKeyConstants.All, cancellationToken);
    }

    // ICacheService only runs the factory on a miss, which is how the two are told apart. The miss
    // is recorded inside the factory so it still counts when the factory throws (product not found).
    private async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken)
    {
        var isMiss = false;

        var value = await cacheService.GetOrSetAsync(
            key,
            ct =>
            {
                isMiss = true;
                CatalogTelemetry.RecordCacheMiss();
                return factory(ct);
            },
            CacheExpiration,
            cancellationToken);

        if (!isMiss)
            CatalogTelemetry.RecordCacheHit();

        return value;
    }
}
