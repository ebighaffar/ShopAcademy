using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class RoleRepository(ShopDbContext context) : RepositoryBase<Role>(context), IRoleRepository
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => Context.Roles.FirstOrDefaultAsync(role => role.Name == name, cancellationToken);

    public Task<Role?> GetByIdWithPermissionsAsync(long id,
        CancellationToken cancellationToken = default)
        => Context.Roles
            .Include(role => role.Permissions)
            .ThenInclude(item => item.Permission)
            .FirstOrDefaultAsync(role => role.Id == id, cancellationToken);
}
