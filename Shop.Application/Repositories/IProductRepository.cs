using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetAllAsync(string? searchPhase, long? categoryId, long? minCount, long? maxPrice,
        CancellationToken cancellationToken);

    IQueryable<Product> GetAllAsQueryable();
}