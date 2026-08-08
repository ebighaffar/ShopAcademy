using Shop.Application.Caching.Constants;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;

namespace Shop.Application.Caching;

public sealed class ProductCache(ICacheService cacheService)
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public Task<IReadOnlyList<ProductDto>> GetAllAsync(
        Func<CancellationToken, Task<IReadOnlyList<ProductDto>>> factory,
        CancellationToken cancellationToken = default)
        => cacheService.GetOrSetAsync(
            ProductCacheKeyConstants.All,
            factory,
            CacheExpiration,
            cancellationToken);

    public Task<ProductDto> GetByIdAsync(
        long id,
        Func<CancellationToken, Task<ProductDto>> factory,
        CancellationToken cancellationToken = default)
        => cacheService.GetOrSetAsync(
            ProductCacheKeyConstants.ById(id),
            factory,
            CacheExpiration,
            cancellationToken);

    public async Task InvalidateAsync(long id, CancellationToken cancellationToken = default)
    {
        await cacheService.RemoveAsync(ProductCacheKeyConstants.ById(id), cancellationToken);
        await cacheService.RemoveAsync(ProductCacheKeyConstants.All, cancellationToken);
    }
}
