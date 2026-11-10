using Serilog;
using Shop.Api;
using Shop.Api.Extensions;
using Shop.Api.Middleware;
using Shop.Api.Telemetry;
using Shop.Application;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Binds the "Sentry" config section; an empty Sentry:Dsn disables reporting.
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Debug = true;
});

// Sinks and levels come from the "Serilog" section in appsettings.json.
    
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteToOpenTelemetry(context.Configuration, context.HostingEnvironment));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi();

// Traces and metrics; the endpoint comes from the "OpenTelemetry" section in appsettings.json.
builder.Services.AddOpenTelemetryExport(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.SeedIdentityAsync();

app.UseApiDocumentation();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapSentryTestEndpoint();

app.Run();
