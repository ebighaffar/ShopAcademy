using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shop.Api.OpenApi;

internal sealed class ConfigureSwaggerOptions(
    IApiVersionDescriptionProvider descriptionProvider
) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the access token returned by login or register."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        foreach (var description in descriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Shop API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                    ? "This API version is deprecated."
                    : "Shop HTTP API."
            });
        }
    }
}
