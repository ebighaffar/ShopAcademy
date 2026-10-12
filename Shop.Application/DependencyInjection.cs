using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Caching;
using Shop.Application.Interfaces;
using Shop.Application.Services;
using Shop.Application.Services.Implementations;
using Shop.Application.Services.Interfaces;

namespace Shop.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductCache>();
        services.AddScoped<ProductService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<ExceptionLogService>();
    }
}