using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Infrastructure.Caching.Redis;
using Shop.Infrastructure.Persistence.Mongo;
using Shop.Infrastructure.Persistence.Mongo.Repositories;
using Shop.Infrastructure.Persistence.PostgreSql.Dapper;
using Shop.Infrastructure.Persistence.PostgreSql.Dapper.Repositories;
using Shop.Infrastructure.Persistence.PostgreSql.EfCore;
using Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

namespace Shop.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Dapper is wired up alongside EF Core: same "Default" connection string, same IProductRepository
        // contract. Registered under its concrete type (not IProductRepository) so it doesn't replace the
        // EF-backed repository above; inject DapperProductRepository directly where the Dapper path is wanted.
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<DapperProductRepository>();

        services.AddMongo(configuration);
        services.AddRedis(configuration);
    }

    // MongoDB coexists with EF Core / Dapper. It backs a separate concern — captured
    // exception logs — so it does not replace any of the SQL-backed repositories.
    private static void AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));

        // Class maps are process-wide; register them once at composition time.
        MongoMappingConfig.Register();

        // The Mongo client manages its own connection pool and is thread-safe, so the
        // context is a singleton.
        services.AddSingleton<MongoContext>();
        services.AddScoped<IExceptionLogRepository, ExceptionLogRepository>();
    }

    // Redis provides the distributed cache behind ICacheService. The connection
    // multiplexer is a shared singleton; the cache service is stateless and cheap,
    // so it is registered as a singleton too.
    private static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));

        services.AddSingleton<RedisConnection>();
        services.AddScoped<ICacheService, RedisCacheService>();
    }
}