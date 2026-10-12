using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class ProductRepository(ShopDbContext context) : RepositoryBase<Product>(context), IProductRepository
{
    public async Task<List<Product>> GetAllAsync(string? searchPhase, long? categoryId, long? minCount, long? maxPrice,
        CancellationToken cancellationToken)
    {
        var query = Context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(searchPhase))
            query = query.Where(p => p.Name.Contains(searchPhase));

        if (categoryId is not null)
            query = query.Where(p => p.CategoryId == categoryId);

        if (minCount is not null)
            query = query.Where(p => p.StockQuantity >= minCount);
        if (minCount is not null)
            query = query.Where(p => p.Price.Amount <= maxPrice);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<long> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        return await Context.Products
            .IgnoreQueryFilters()
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public IQueryable<Product> GetAllAsQueryable()
    {
        return Context.Products.AsNoTracking();
    }
}
