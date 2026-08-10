namespace Shop.Api.Constants;

/// <summary>
/// Contains URI constant values for exception log-related endpoints.
/// </summary>
public static class ExceptionLogUriConstants
{
    private const string Controller = "exceptionlogs";

    /// <summary>
    /// Represents the endpoint for retrieving all exception logs.
    /// </summary>
    public const string GetAll = $"{Controller}";

    /// <summary>
    /// Represents the endpoint for retrieving recent exception logs.
    /// </summary>
    public const string GetRecent = $"{Controller}/recent";

    /// <summary>
    /// Represents the endpoint for retrieving a specific exception log by ID.
    /// </summary>
    public const string GetById = $"{Controller}/{{id}}";
}