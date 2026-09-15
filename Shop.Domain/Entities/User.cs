using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class User
{
    private readonly List<UserRole> _roles = [];
    private readonly List<UserPermission> _permissions = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    public long Id { get; private set; }
    public string UserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public AddressValueObject? Address { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Basket? Basket { get; private set; }
    public IReadOnlyCollection<UserRole> Roles => _roles;
    public IReadOnlyCollection<UserPermission> Permissions => _permissions;
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    private User()
    {
    }

    public User(string firstName, string lastName, string phoneNumber, string userName,
        string email)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UserName = userName.Trim().ToLowerInvariant();
        Email = email.Trim().ToLowerInvariant();
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void ChangeEmail(string email) => Email = email.Trim().ToLowerInvariant();

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void AssignRole(Role role)
    {
        if (_roles.All(userRole => userRole.RoleId != role.Id))
            _roles.Add(new UserRole(this, role));
    }

    public void RemoveRole(long roleId)
        => _roles.RemoveAll(userRole => userRole.RoleId == roleId);

    public void AssignPermission(Permission permission)
    {
        if (_permissions.All(item => item.PermissionId != permission.Id))
            _permissions.Add(new UserPermission(this, permission));
    }

    public void RemovePermission(long permissionId)
        => _permissions.RemoveAll(item => item.PermissionId == permissionId);

    public void SetAddress(AddressValueObject address)
    {
        Address = address;
    }

    public Basket GetOrCreateBasket(long basketId)
    {
        Basket ??= new Basket(basketId, Id);
        return Basket;
    }
}
