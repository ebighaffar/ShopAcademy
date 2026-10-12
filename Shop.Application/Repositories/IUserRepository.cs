using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithBasketAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithPermissionsAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByLoginWithRolesAsync(string login, CancellationToken cancellationToken = default);
    public Task<User?> GetByIdIncludeBasketAsync(long userId, CancellationToken cancellationToken);
}
