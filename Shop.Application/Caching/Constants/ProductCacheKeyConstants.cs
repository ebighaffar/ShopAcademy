namespace Shop.Application.Caching.Constants;

public static class ProductCacheKeyConstants
{
    public const string All = "products:all";

    public static string ById(long id) => $"products:{id}";
}
