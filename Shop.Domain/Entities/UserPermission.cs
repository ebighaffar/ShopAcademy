namespace Shop.Domain.Entities;

public class UserPermission
{
    public long UserId { get; private set; }
    public long PermissionId { get; private set; }
    public User User { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private UserPermission()
    {
    }

    internal UserPermission(User user, Permission permission)
    {
        User = user;
        UserId = user.Id;
        Permission = permission;
        PermissionId = permission.Id;
    }
}
