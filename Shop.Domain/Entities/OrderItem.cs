using Shop.Domain.Exceptions;
using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class OrderItem
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public long ProductId { get; private set; }

    // Name and price are snapshots taken at placement, so history survives later
    // product renames, price changes and deletion.
    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }

    public decimal Subtotal => UnitPrice.Amount * Quantity;

    private OrderItem()
    {
    }

    internal OrderItem(long productId, string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
