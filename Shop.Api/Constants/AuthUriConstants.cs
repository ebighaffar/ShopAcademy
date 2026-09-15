namespace Shop.Api.Constants;

/// <summary>
/// Contains URI constant values for authentication endpoints.
/// </summary>
public static class AuthUriConstants
{
    private const string Controller = "auth";

    /// <summary>
    /// Represents the user registration endpoint.
    /// </summary>
    public const string Register = $"{Controller}/register";

    /// <summary>
    /// Represents the user login endpoint.
    /// </summary>
    public const string Login = $"{Controller}/login";

    /// <summary>
    /// Represents the access-token refresh endpoint.
    /// </summary>
    public const string Refresh = $"{Controller}/refresh";

    /// <summary>
    /// Represents the current-session logout endpoint.
    /// </summary>
    public const string Logout = $"{Controller}/logout";

    /// <summary>
    /// Represents the endpoint that logs out every user session.
    /// </summary>
    public const string LogoutAll = $"{Controller}/logout-all";

    /// <summary>
    /// Represents the current authenticated user endpoint.
    /// </summary>
    public const string Me = $"{Controller}/me";
}
