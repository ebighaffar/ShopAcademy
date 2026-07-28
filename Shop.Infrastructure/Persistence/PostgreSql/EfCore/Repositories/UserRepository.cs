using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class UserRepository(ShopDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public Task<User?> GetByIdWithBasketAsync(long id, CancellationToken cancellationToken = default)
        => Context.Users
            .Include(u => u.Basket)
            .ThenInclude(b => b!.Items)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
}