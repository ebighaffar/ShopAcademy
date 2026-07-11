using Shop.Domain.Entities;

namespace Shop.Application.Repository;

public interface IUserRepository : IRepository<User, long>
{
    Task<User?> GetByIdWithBasketAsync(long id, CancellationToken cancellationToken = default);
}
