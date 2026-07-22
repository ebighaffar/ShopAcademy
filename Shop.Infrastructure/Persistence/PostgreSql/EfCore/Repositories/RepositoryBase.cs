using Microsoft.EntityFrameworkCore;
using Shop.Application.Repository;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public abstract class RepositoryBase<TEntity>(ShopDbContext context) : IRepository<TEntity>
    where TEntity : class
{
    protected readonly ShopDbContext Context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(TEntity entity)
        => _dbSet.Update(entity);

    public virtual void Remove(TEntity entity)
        => _dbSet.Remove(entity);

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}