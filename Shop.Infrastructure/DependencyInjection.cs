using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Interfaces;
using Shop.Application.Repository;
using Shop.Infrastructure.Persistence.SqlServer;
using Shop.Infrastructure.Persistence.SqlServer.Dapper;
using Shop.Infrastructure.Persistence.SqlServer.Dapper.Repositories;
using Shop.Infrastructure.Persistence.SqlServer.EfCore;
using Shop.Infrastructure.Persistence.SqlServer.EfCore.Repositories;
using Shop.Infrastructure.Security;

namespace Shop.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ShopDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddTransient<IPasswordHasher, PasswordHasher>();

        // Dapper is wired up alongside EF Core: same "Default" connection string, same IProductRepository
        // contract. Registered under its concrete type (not IProductRepository) so it doesn't replace the
        // EF-backed repository above; inject DapperProductRepository directly where the Dapper path is wanted.
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<DapperProductRepository>();
    }
}