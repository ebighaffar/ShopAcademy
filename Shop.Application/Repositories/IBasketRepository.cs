using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IBasketRepository : IRepository<Basket>
{
    Task<Basket?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
