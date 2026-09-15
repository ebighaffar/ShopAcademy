using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class PermissionRepository(ShopDbContext context)
    : RepositoryBase<Permission>(context), IPermissionRepository
{
    public Task<Permission?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return Context.Permissions.FirstOrDefaultAsync(
            permission => permission.Name == normalized, cancellationToken);
    }
}
