using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(long id,
        CancellationToken cancellationToken = default);
}
