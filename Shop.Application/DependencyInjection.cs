using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Services;

namespace Shop.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<UserService>();
    }
}
