namespace Shop.Domain.Entities;

public class Basket
{
    private readonly List<BasketItem> _items = [];

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    private Basket()
    {
    }

    public Basket(long id, long userId)
    {
        Id = id;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AddItem(Product product, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return;
        }

        _items.Add(new BasketItem(_items.Count + 1, Id, product.Id, quantity));
    }

    public void RemoveItem(long productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
