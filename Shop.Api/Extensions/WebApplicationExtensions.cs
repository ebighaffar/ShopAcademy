using Scalar.AspNetCore;
using Shop.Infrastructure.Persistence.PostgreSql.EfCore;

namespace Shop.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
        await seeder.SeedAsync();
    }

    /// <summary>
    /// Development-only endpoint that throws, to verify an error reaches the Sentry dashboard.
    /// </summary>
    public static void MapSentryTestEndpoint(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.MapGet("/debug/sentry-test", () =>
        {
            throw new InvalidOperationException("Sentry test exception");
        }).AllowAnonymous();
    }

    public static void UseApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.MapOpenApi().WithDocumentPerVersion();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Shop API");

            var descriptions = app.DescribeApiVersions();
            for (var index = 0; index < descriptions.Count; index++)
            {
                var description = descriptions[index];
                options.AddDocument(
                    description.GroupName,
                    description.GroupName.ToUpperInvariant(),
                    isDefault: index == 0);
            }
        });

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DisplayRequestDuration();

            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
    }
}
