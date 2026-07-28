using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

/// <summary>
/// Append-and-read store for captured exceptions. Backed by MongoDB, so it does
/// not extend <see cref="IRepository{TEntity}"/> (no unit-of-work / SaveChanges);
/// writes are committed immediately by the driver.
/// </summary>
public interface IExceptionLogRepository
{
    Task AddAsync(ExceptionLog log, CancellationToken cancellationToken = default);
    Task<ExceptionLog?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExceptionLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExceptionLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}
