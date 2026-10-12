using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class BasketRepository(ShopDbContext context) : RepositoryBase<Basket>(context), IBasketRepository
{
    public Task<Basket?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
        => Context.Baskets
            .Include(basket => basket.Items)
            .FirstOrDefaultAsync(basket => basket.UserId == userId, cancellationToken);
}
