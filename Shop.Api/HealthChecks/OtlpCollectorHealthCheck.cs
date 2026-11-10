using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shop.Api.Telemetry;

namespace Shop.Api.HealthChecks;

/// <summary>
/// Reports whether the OpenTelemetry collector accepts TCP connections on the configured OTLP
/// endpoint (works for both gRPC and HTTP). Healthy when export is turned off.
/// Registered with <see cref="HealthStatus.Degraded"/> as its failure status: telemetry is exported in
/// the background, so a collector outage must not take the API out of rotation.
/// </summary>
public sealed class OtlpCollectorHealthCheck(IConfiguration configuration) : IHealthCheck
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(2);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var settings = configuration.GetSection(OpenTelemetrySettings.SectionName).Get<OpenTelemetrySettings>()
                       ?? new OpenTelemetrySettings();

        if (!settings.IsEnabled)
            return HealthCheckResult.Healthy("OpenTelemetry export is disabled.");

        if (!Uri.TryCreate(settings.Otlp.Endpoint, UriKind.Absolute, out var endpoint))
            return new HealthCheckResult(context.Registration.FailureStatus, "OTLP endpoint is not a valid URI.");

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken).AsTask()
                .WaitAsync(ConnectTimeout, cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "OTLP collector is unreachable.",
                exception);
        }
    }
}
