using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Mongo;

/// <summary>
/// Owns the <see cref="IMongoDatabase"/> and exposes strongly-typed collections.
/// Registered as a singleton — the MongoDB driver's <see cref="IMongoClient"/> is
/// thread-safe and manages its own connection pool.
/// </summary>
public class MongoContext
{
    private readonly MongoSettings _settings;

    public MongoContext(IOptions<MongoSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        Database = client.GetDatabase(_settings.Database);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<ExceptionLog> ExceptionLogs =>
        Database.GetCollection<ExceptionLog>(_settings.ExceptionLogsCollection);
}
