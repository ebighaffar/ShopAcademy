namespace Shop.Api.Telemetry;

/// <summary>
/// Bound from the "OpenTelemetry" config section. A blank <see cref="OtlpSettings.Endpoint"/>
/// turns all telemetry export off.
/// </summary>
public sealed class OpenTelemetrySettings
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; set; } = "ShopAPI";
    public string ServiceVersion { get; set; } = "1.0.0";
    public OtlpSettings Otlp { get; set; } = new();
    public TracingSettings Tracing { get; set; } = new();

    public bool IsEnabled => !string.IsNullOrWhiteSpace(Otlp.Endpoint);

    public sealed class OtlpSettings
    {
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>"grpc" (port 4317) or "http" (port 4318).</summary>
        public string Protocol { get; set; } = "grpc";
    }

    public sealed class TracingSettings
    {
        /// <summary>Share of new traces to keep, from 0.0 to 1.0.</summary>
        public double SamplingRatio { get; set; } = 1.0;
    }
}
