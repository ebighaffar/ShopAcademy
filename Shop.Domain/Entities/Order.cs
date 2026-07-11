using Shop.Domain.Enums;
using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public long Id { get; private set; }
    public long UserId { get; private init; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
    }

    public Order(long id, long userId)
    {
        Id = id;
        UserId = userId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AddItem(Product product, int quantity)
    {
        var item = new OrderItem(_items.Count + 1, Id, product.Id, product.Price, quantity);
        _items.Add(item);
    }

    public Money Total()
    {
        var currency = _items.Count > 0 ? _items[0].UnitPrice.Currency : default;
        var total = new Money(0, currency);
        foreach (var item in _items)
            total = total.Add(new Money(item.Subtotal, item.UnitPrice.Currency));

        return total;
    }
}
