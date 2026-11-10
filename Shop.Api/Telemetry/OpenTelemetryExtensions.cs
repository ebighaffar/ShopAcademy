using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using Shop.Application.Telemetry;

namespace Shop.Api.Telemetry;

public static class OpenTelemetryExtensions
{
    private static readonly string[] UntracedPathPrefixes = ["/swagger", "/scalar", "/openapi"];

    /// <summary>
    /// Registers traces and metrics. Logs are not part of this: Serilog replaces the logging
    /// providers, so they are exported by <see cref="WriteToOpenTelemetry"/> instead.
    /// </summary>
    public static void AddOpenTelemetryExport(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var settings = GetSettings(configuration);
        if (!settings.IsEnabled)
            return;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddAttributes(BuildResourceAttributes(settings, environment)))
            .WithTracing(tracing => tracing
                .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(settings.Tracing.SamplingRatio)))
                .AddSource(AuthTelemetry.Name, CatalogTelemetry.Name)
                .AddAspNetCoreInstrumentation(options => options.Filter = IsTraced)
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddRedisInstrumentation()
                .AddOtlpExporter(options => ConfigureExporter(options, settings, "/v1/traces")))
            .WithMetrics(metrics => metrics
                .AddMeter(AuthTelemetry.Name, CatalogTelemetry.Name)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => ConfigureExporter(options, settings, "/v1/metrics")));
    }

    /// <summary>
    /// Adds the OTLP log sink, using the same endpoint and service identity as traces and metrics
    /// so that all three signals line up in the dashboard.
    /// </summary>
    public static LoggerConfiguration WriteToOpenTelemetry(
        this LoggerConfiguration logger,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var settings = GetSettings(configuration);
        if (!settings.IsEnabled)
            return logger;

        return logger.WriteTo.OpenTelemetry(options =>
        {
            options.Protocol = UsesHttp(settings) ? OtlpProtocol.HttpProtobuf : OtlpProtocol.Grpc;
            options.Endpoint = settings.Otlp.Endpoint;
            options.ResourceAttributes = BuildResourceAttributes(settings, environment);
        });
    }

    private static OpenTelemetrySettings GetSettings(IConfiguration configuration)
        => configuration.GetSection(OpenTelemetrySettings.SectionName).Get<OpenTelemetrySettings>()
           ?? new OpenTelemetrySettings();

    private static Dictionary<string, object> BuildResourceAttributes(
        OpenTelemetrySettings settings,
        IHostEnvironment environment)
        => new()
        {
            ["service.name"] = settings.ServiceName,
            ["service.version"] = settings.ServiceVersion,
            ["service.instance.id"] = Environment.MachineName,
            ["deployment.environment.name"] = environment.EnvironmentName.ToLowerInvariant()
        };

    private static void ConfigureExporter(OtlpExporterOptions options, OpenTelemetrySettings settings, string httpPath)
    {
        var endpoint = settings.Otlp.Endpoint.TrimEnd('/');

        if (UsesHttp(settings))
        {
            // Over HTTP each signal has its own path, and the exporter only adds it for environment variables.
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            options.Endpoint = new Uri(endpoint + httpPath);
            return;
        }

        options.Protocol = OtlpExportProtocol.Grpc;
        options.Endpoint = new Uri(endpoint);
    }

    private static bool UsesHttp(OpenTelemetrySettings settings)
        => settings.Otlp.Protocol.Equals("http", StringComparison.OrdinalIgnoreCase);

    private static bool IsTraced(HttpContext context)
        => !UntracedPathPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix));
}
