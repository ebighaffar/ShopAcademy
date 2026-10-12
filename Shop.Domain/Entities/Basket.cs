using Shop.Domain.Exceptions;

namespace Shop.Domain.Entities;

public class Basket
{
    private readonly List<BasketItem> _items = [];

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();
    public bool IsEmpty => _items.Count == 0;

    private Basket()
    {
    }

    public Basket(long userId)
    {
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // The basket does not reserve stock; it only refuses quantities that could never be
    // fulfilled right now. Order.Place re-checks (and decreases) stock at checkout.
    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();
        EnsureAvailable(product);

        var existingItem = FindItem(product.Id);
        var newQuantity = (existingItem?.Quantity ?? 0) + quantity;
        EnsureInStock(product, newQuantity);

        if (existingItem is null)
            _items.Add(new BasketItem(product.Id, quantity));
        else
            existingItem.ChangeQuantity(newQuantity);
    }

    public void UpdateQuantity(Product product, int quantity)
    {
        var item = FindItem(product.Id) ?? throw new BasketItemNotFoundException(product.Id);
        if (quantity <= 0)
            throw new InvalidQuantityException();
        EnsureAvailable(product);
        EnsureInStock(product, quantity);

        item.ChangeQuantity(quantity);
    }

    public void RemoveItem(long productId)
    {
        var item = FindItem(productId) ?? throw new BasketItemNotFoundException(productId);
        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    private BasketItem? FindItem(long productId) => _items.FirstOrDefault(i => i.ProductId == productId);

    private static void EnsureAvailable(Product product)
    {
        if (!product.IsAvailableForSale)
            throw new ProductNotAvailableException(product.Id);
    }

    private static void EnsureInStock(Product product, int quantity)
    {
        if (quantity > product.StockQuantity)
            throw new InsufficientStockException(product.Id, quantity, product.StockQuantity);
    }
}
