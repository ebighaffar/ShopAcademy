namespace Shop.Domain.Entities;

public class RolePermission
{
    public long RoleId { get; private set; }
    public long PermissionId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission()
    {
    }

    internal RolePermission(Role role, Permission permission)
    {
        Role = role;
        RoleId = role.Id;
        Permission = permission;
        PermissionId = permission.Id;
    }
}
