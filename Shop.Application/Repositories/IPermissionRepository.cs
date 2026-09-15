using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default);
}
