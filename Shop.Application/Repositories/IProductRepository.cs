using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetAllAsync(string? searchPhase, long? categoryId, long? minCount, long? maxPrice,
        CancellationToken cancellationToken);

    // Includes soft-deleted products: order history and cancellation must still reach them.
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    IQueryable<Product> GetAllAsQueryable();
}
