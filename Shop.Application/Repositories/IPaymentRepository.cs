using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
}
