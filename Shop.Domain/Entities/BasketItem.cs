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

    internal BasketItem(long id, long basketId, long productId, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Id = id;
        BasketId = basketId;
        ProductId = productId;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new InvalidQuantityException();

        Quantity += amount;
    }
}
