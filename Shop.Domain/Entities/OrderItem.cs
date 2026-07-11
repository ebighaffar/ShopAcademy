using Shop.Domain.Exceptions;
using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class OrderItem
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public long ProductId { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }

    public decimal Subtotal => UnitPrice.Amount * Quantity;

    private OrderItem()
    {
    }

    internal OrderItem(long id, long orderId, long productId, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Id = id;
        OrderId = orderId;
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
