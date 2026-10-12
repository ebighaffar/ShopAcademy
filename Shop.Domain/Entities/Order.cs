using Shop.Domain.Constants;
using Shop.Domain.Enums;
using Shop.Domain.Exceptions;
using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public AddressValueObject ShippingAddress { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
    }

    private Order(long userId, AddressValueObject shippingAddress)
    {
        UserId = userId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Checkout: turns a basket into an order and takes the stock out of the products.
    // It does not clear the basket; the caller does that in the same unit of work.
    public static Order Place(
        long userId,
        Basket basket,
        IReadOnlyDictionary<long, Product> productsById,
        AddressValueObject shippingAddress)
    {
        ArgumentNullException.ThrowIfNull(basket);
        ArgumentNullException.ThrowIfNull(shippingAddress);
        if (basket.IsEmpty)
            throw new EmptyBasketException();

        var order = new Order(userId, shippingAddress);
        foreach (var basketItem in basket.Items)
        {
            if (!productsById.TryGetValue(basketItem.ProductId, out var product))
                throw new EntityNotFoundException(nameof(Product), basketItem.ProductId);
            if (!product.IsAvailableForSale)
                throw new ProductNotAvailableException(product.Id);

            product.DecreaseStock(basketItem.Quantity);
            order._items.Add(new OrderItem(product.Id, product.Name, product.Price, basketItem.Quantity));
        }

        return order;
    }

    public Money Total()
    {
        var total = new Money(0, ShopConstants.Currency);
        foreach (var item in _items)
            total += new Money(item.Subtotal, item.UnitPrice.Currency);

        return total;
    }

    public Payment CreatePayment(PaymentMethod method)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Status, "create a payment for");

        return new Payment(Id, Total(), method);
    }

    public void MarkPaid(Payment payment)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Status, "pay");
        if (payment.OrderId != Id)
            throw new InvalidPaymentException("Payment belongs to a different order");
        if (payment.Status != PaymentStatus.Succeeded)
            throw new InvalidPaymentException("Payment has not succeeded");
        if (payment.Amount != Total())
            throw new InvalidPaymentException("Payment amount does not match the order total");

        Status = OrderStatus.Paid;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOrderStateException(Status, "ship");

        Status = OrderStatus.Shipped;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOrderStateException(Status, "complete");

        Status = OrderStatus.Completed;
    }

    // Cancelling gives the stock back. Refunding an already-paid order is not modelled yet.
    public void Cancel(IReadOnlyDictionary<long, Product> productsById)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Paid))
            throw new InvalidOrderStateException(Status, "cancel");

        foreach (var item in _items)
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
                throw new EntityNotFoundException(nameof(Product), item.ProductId);

            product.IncreaseStock(item.Quantity);
        }

        Status = OrderStatus.Cancelled;
    }
}
