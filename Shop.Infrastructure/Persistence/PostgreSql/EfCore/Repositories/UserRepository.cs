using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class UserRepository(ShopDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public Task<User?> GetByIdWithBasketAsync(long id, CancellationToken cancellationToken = default)
        => Context.Users
            .Include(user => user.Basket)
            .ThenInclude(basket => basket!.Items)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim().ToLowerInvariant();
        return Context.Users.FirstOrDefaultAsync(
            user => user.UserName == normalized, cancellationToken);
    }

    public Task<User?> GetByIdWithRolesAsync(long id,
        CancellationToken cancellationToken = default)
        => Context.Users
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return Context.Users.FirstOrDefaultAsync(
            user => user.Email == normalized, cancellationToken);
    }

    public Task<User?> GetByIdWithPermissionsAsync(long id,
        CancellationToken cancellationToken = default)
        => Context.Users
            .Include(user => user.Permissions)
            .ThenInclude(userPermission => userPermission.Permission)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByLoginWithRolesAsync(string login,
        CancellationToken cancellationToken = default)
    {
        var normalized = login.Trim().ToLowerInvariant();
        return Context.Users
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                user => user.UserName == normalized || user.Email == normalized,
                cancellationToken);
    }

    public async Task<User?> GetByIdIncludeBasketAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await Context.Users
            .Include(u => u.Basket)
            .ThenInclude(ub => ub!.Items)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user;
    }
}