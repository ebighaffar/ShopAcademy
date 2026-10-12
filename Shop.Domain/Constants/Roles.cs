namespace Shop.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Support = "Support";

    public static readonly IReadOnlyCollection<string> SystemRoles =
    [
        Admin,
        Manager,
        User,
        Support
    ];
}