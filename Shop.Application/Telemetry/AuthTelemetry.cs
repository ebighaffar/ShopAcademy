using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Shop.Application.Telemetry;

/// <summary>
/// Custom spans and metrics for authentication. <see cref="Name"/> must be registered with
/// AddSource and AddMeter, or everything here is silently dropped.
/// </summary>
public static class AuthTelemetry
{
    public const string Name = "ShopAPI.Auth";

    public const string OutcomeTag = "outcome";
    public const string OutcomeSuccess = "success";
    public const string OutcomeFailed = "failed";
    public const string OutcomeError = "error";

    public static readonly ActivitySource Source = new(Name);

    private static readonly Meter Meter = new(Name);

    private static readonly Counter<long> Logins = Meter.CreateCounter<long>(
        "shop.auth.logins",
        description: "Login attempts by outcome.");

    private static readonly Histogram<double> LoginDuration = Meter.CreateHistogram<double>(
        "shop.auth.login.duration",
        unit: "ms",
        description: "Time taken to handle a login attempt.");

    /// <param name="outcome">One of the Outcome constants. Never a user id: metric tags must stay bounded.</param>
    /// <param name="elapsedMilliseconds">How long the attempt took.</param>
    public static void RecordLogin(string outcome, double elapsedMilliseconds)
    {
        var tag = new KeyValuePair<string, object?>(OutcomeTag, outcome);

        Logins.Add(1, tag);
        LoginDuration.Record(elapsedMilliseconds, tag);
    }
}
