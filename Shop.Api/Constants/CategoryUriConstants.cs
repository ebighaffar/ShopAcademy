namespace Shop.Api.Constants;

/// <summary>
/// Contains URI constant values for category-related endpoints.
/// </summary>
public static class CategoryUriConstants
{
    private const string Controller = "categories";

    /// <summary>
    /// Represents the endpoint for retrieving all categories.
    /// </summary>
    public const string GetAll = $"{Controller}";

    /// <summary>
    /// Represents the endpoint for retrieving a specific category by ID.
    /// </summary>
    public const string GetById = $"{Controller}/{{id}}";

    /// <summary>
    /// Represents the endpoint for creating a new category.
    /// </summary>
    public const string Create = $"{Controller}";

    /// <summary>
    /// Represents the endpoint for updating an existing category.
    /// </summary>
    public const string Update = $"{Controller}/{{id}}";

    /// <summary>
    /// Represents the endpoint for deleting a category.
    /// </summary>
    public const string Delete = $"{Controller}/{{id}}";
}