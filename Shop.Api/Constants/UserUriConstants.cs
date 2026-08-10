namespace Shop.Api.Constants;

/// <summary>
/// Contains URI constant values for user-related endpoints.
/// </summary>
public static class UserUriConstants
{
    private const string Controller = "users";

    /// <summary>
    /// Represents the endpoint for user registration.
    /// </summary>
    public const string Register = $"{Controller}/register";

    /// <summary>
    /// Represents the endpoint for retrieving a specific user by ID.
    /// </summary>
    public const string GetById = $"{Controller}/{{id}}";

    /// <summary>
    /// Represents the endpoint for updating an existing user.
    /// </summary>
    public const string Update = $"{Controller}/{{id}}";

    /// <summary>
    /// Represents the endpoint for deleting a user.
    /// </summary>
    public const string Delete = $"{Controller}/{{id}}";
}