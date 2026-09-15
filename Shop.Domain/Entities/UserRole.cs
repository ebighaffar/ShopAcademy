namespace Shop.Domain.Entities;

public class UserRole
{
    public long UserId { get; private set; }
    public long RoleId { get; private set; }
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole()
    {
    }

    internal UserRole(User user, Role role)
    {
        User = user;
        UserId = user.Id;
        Role = role;
        RoleId = role.Id;
    }
}
