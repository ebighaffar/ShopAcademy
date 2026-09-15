using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Persistence.PostgreSql.EfCore;

namespace Shop.Infrastructure.Authorization;

public class UserPermissionService(ShopDbContext context) : IUserPermissionService
{
    public async Task<IReadOnlySet<string>> GetPermissionsAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        var rolePermissions = context.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role.Permissions)
            .Select(rolePermission => rolePermission.Permission.Name);

        var directPermissions = context.UserPermissions
            .Where(userPermission => userPermission.UserId == userId)
            .Select(userPermission => userPermission.Permission.Name);

        var permissions = await rolePermissions
            .Concat(directPermissions)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions.ToHashSet(StringComparer.Ordinal);
    }
}
