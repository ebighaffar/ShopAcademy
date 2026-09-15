using Microsoft.EntityFrameworkCore;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore;

public class IdentityDataSeeder(ShopDbContext context)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var roles = await context.Roles
            .Include(role => role.Permissions)
            .ToListAsync(cancellationToken);
        var roleByName = roles.ToDictionary(role => role.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in Roles.SystemRoles)
        {
            if (!roleByName.TryGetValue(roleName, out var role))
            {
                role = new Role(roleName, isSystem: true);
                context.Roles.Add(role);
                roleByName.Add(roleName, role);
            }
            else
            {
                role.MarkAsSystem();
            }
        }

        var permissions = await context.Permissions.ToListAsync(cancellationToken);
        var permissionByName = permissions.ToDictionary(
            permission => permission.Name,
            StringComparer.OrdinalIgnoreCase);

        foreach (var permissionName in Permissions.SystemPermissions)
        {
            if (!permissionByName.TryGetValue(permissionName, out var permission))
            {
                permission = new Permission(permissionName, isSystem: true);
                context.Permissions.Add(permission);
                permissionByName.Add(permissionName, permission);
            }
            else
            {
                permission.MarkAsSystem();
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        AddMissingPermissions(roleByName[Roles.Admin], Permissions.SystemPermissions, permissionByName);
        AddMissingPermissions(roleByName[Roles.Manager],
            [
                Permissions.Products.Read,
                Permissions.Products.Manage,
                Permissions.Categories.Manage,
                Permissions.Users.Read
            ],
            permissionByName);
        AddMissingPermissions(roleByName[Roles.User],
            [Permissions.Products.Read],
            permissionByName);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void AddMissingPermissions(
        Role role,
        IEnumerable<string> permissionNames,
        IReadOnlyDictionary<string, Permission> permissionByName)
    {
        foreach (var permissionName in permissionNames)
            role.AssignPermission(permissionByName[permissionName]);
    }
}
