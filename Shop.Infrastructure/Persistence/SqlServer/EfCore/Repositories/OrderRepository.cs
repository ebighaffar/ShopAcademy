using Microsoft.EntityFrameworkCore;
using Shop.Application.Repository;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.SqlServer.EfCore.Repositories;

public class OrderRepository(ShopDbContext context) : RepositoryBase<Order, long>(context), IOrderRepository
{
    public override Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
}
