namespace Shop.Api.Constants;

/// <summary>
/// Contains URI constant values for the health probe endpoints.
/// </summary>
public static class HealthUriConstants
{
    private const string Root = "health";

    /// <summary>
    /// Liveness probe: the process is up and able to answer. Checks no dependency.
    /// </summary>
    public const string Live = $"{Root}/live";

    /// <summary>
    /// Readiness probe: the dependencies the API needs to serve traffic (PostgreSQL, Redis) are reachable.
    /// </summary>
    public const string Ready = $"{Root}/ready";
}
