using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithBasketAsync(long id, CancellationToken cancellationToken = default);
}
