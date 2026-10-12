using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class PaymentRepository(ShopDbContext context) : RepositoryBase<Payment>(context), IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(long orderId,
        CancellationToken cancellationToken = default)
        => await Context.Payments
            .Where(payment => payment.OrderId == orderId)
            .OrderBy(payment => payment.CreatedAt)
            .ToListAsync(cancellationToken);
}
