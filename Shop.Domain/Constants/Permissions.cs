namespace Shop.Domain.Constants;

public static class Permissions
{
    public static class Products
    {
        public const string Read = "products.read";
        public const string Manage = "products.manage";
    }

    public static class Users
    {
        public const string Read = "users.read";
        public const string Manage = "users.manage";
    }

    public static class Categories
    {
        public const string Manage = "categories.manage";
    }

    public static readonly IReadOnlyCollection<string> SystemPermissions =
    [
        Products.Read,
        Products.Manage,
        Categories.Manage,
        Users.Read,
        Users.Manage
    ];
}
