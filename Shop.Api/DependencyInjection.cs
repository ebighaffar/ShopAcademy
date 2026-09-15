using System.Text;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Shop.Api.Authentication;
using Shop.Api.Filters;
using Shop.Api.OpenApi;
using Shop.Infrastructure.Authentication;

namespace Shop.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddJwtAuthentication();
        services.AddAuthorizationPolicies();
        services.AddApiVersioningAndOpenApi();
        services.AddControllersAndValidation();

        return services;
    }

    private static void AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var settings = jwtOptions.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key))
                };
            });
    }

    private static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(AuthorizationPolicies.AddPolicies);
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    private static void AddApiVersioningAndOpenApi(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ReportApiVersions = true;
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi(options => options.Document.AddScalarTransformers());

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();
    }

    private static void AddControllersAndValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
            options.Filters.Add<ApiResultFilter>();
        });
    }
}