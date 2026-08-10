namespace Shop.Api.Constants;

/// <summary>
/// Contains URI constant values for product-related endpoints.
/// </summary>
public static class ProductUriConstants
{
    private const string Controller = "products";

    /// <summary>
    /// Represents the endpoint for retrieving all products.
    /// </summary>
    public const string GetAll = $"{Controller}";

    /// <summary>
    /// Represents the endpoint for retrieving a specific product by ID.
    /// </summary>
    public const string GetById = $"{Controller}/{{id}}";

    /// <summary>
    /// Represents the endpoint for creating a new product.
    /// </summary>
    public const string Create = $"{Controller}";

    /// <summary>
    /// Represents the endpoint for updating an existing product.
    /// </summary>
    public const string Update = $"{Controller}/{{id}}";

    /// <summary>
    /// Represents the endpoint for deleting a product.
    /// </summary>
    public const string Delete = $"{Controller}/{{id}}";
}