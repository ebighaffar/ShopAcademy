namespace Shop.Domain.Entities;

public class Role
{
    private readonly List<RolePermission> _permissions = [];

    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsSystem { get; private set; }
    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    private Role()
    {
    }

    public Role(string name, bool isSystem = false)
    {
        Name = name.Trim();
        IsSystem = isSystem;
    }

    public void MarkAsSystem() => IsSystem = true;

    public void AssignPermission(Permission permission)
    {
        if (_permissions.All(item => item.PermissionId != permission.Id))
            _permissions.Add(new RolePermission(this, permission));
    }

    public void RemovePermission(long permissionId)
        => _permissions.RemoveAll(item => item.PermissionId == permissionId);
}
