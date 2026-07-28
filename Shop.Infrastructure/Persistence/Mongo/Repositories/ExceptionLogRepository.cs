using MongoDB.Driver;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Mongo.Repositories;

/// <summary>
/// MongoDB-backed store for <see cref="ExceptionLog"/>. The driver assigns a
/// generated ObjectId on insert; reads are ordered newest-first by creation time.
/// </summary>
public class ExceptionLogRepository(MongoContext context) : IExceptionLogRepository
{
    private IMongoCollection<ExceptionLog> Collection => context.ExceptionLogs;

    public Task AddAsync(ExceptionLog log, CancellationToken cancellationToken = default)
        => Collection.InsertOneAsync(log, options: null, cancellationToken);

    public async Task<ExceptionLog?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var method1 = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        // return method1;

        var method2 = await Collection.FindAsync(
            Builders<ExceptionLog>.Filter.Eq(x => x.Id, id),
            cancellationToken: cancellationToken);

        return await method2.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExceptionLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection
            .Find(Builders<ExceptionLog>.Filter.Empty)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExceptionLog>> GetRecentAsync(int count,
        CancellationToken cancellationToken = default)
    {
        return await Collection
            .Find(Builders<ExceptionLog>.Filter.Empty)
            .SortByDescending(x => x.CreatedAt)
            .Limit(count)
            .ToListAsync(cancellationToken);
    }
}