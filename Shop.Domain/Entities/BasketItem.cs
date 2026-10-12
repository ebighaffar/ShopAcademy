using Shop.Domain.Exceptions;

namespace Shop.Domain.Entities;

public class BasketItem
{
    public long Id { get; private set; }
    public long BasketId { get; private set; }
    public long ProductId { get; private set; }
    public int Quantity { get; private set; }

    private BasketItem()
    {
    }

    internal BasketItem(long productId, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        ProductId = productId;
        Quantity = quantity;
    }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Quantity = quantity;
    }
}
