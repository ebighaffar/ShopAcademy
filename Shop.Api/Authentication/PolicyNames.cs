namespace Shop.Api.Authentication;

public static class PolicyNames
{
    public const string CustomerAccess = "CustomerAccess";
    public const string ActiveUser = "ActiveUser";
    public const string CanReadProducts = "CanReadProducts";
    public const string CanManageProducts = "CanManageProducts";
    public const string CanManageCategories = "CanManageCategories";
}
