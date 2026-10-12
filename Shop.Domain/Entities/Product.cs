using Shop.Domain.Constants;
using Shop.Domain.Exceptions;
using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class Product
{
    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public long CategoryId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Optimistic concurrency token (PostgreSQL xmin): two checkouts racing for the last
    // item cannot both succeed, the second SaveChanges fails.
    public uint Version { get; private set; }

    public Category? Category { get; private init; }

    private Product()
    {
    }

    public Product(string name, string? description, Money price, int stockQuantity, long categoryId)
    {
        EnsureValidName(name);
        EnsureShopCurrency(price);
        if (stockQuantity < 0)
            throw new DomainValidationException("Stock quantity cannot be negative");

        Name = name.Trim();
        Description = description;
        IsActive = true;
        IsDeleted = false;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsAvailableForSale => IsActive && !IsDeleted;

    public void Update(string name, string? description, long categoryId)
    {
        EnsureValidName(name);

        Name = name.Trim();
        Description = description;
        CategoryId = categoryId;
    }

    public void ToggleStatus()
    {
        IsActive = !IsActive;
    }

    public void ChangePrice(Money newPrice)
    {
        EnsureShopCurrency(newPrice);
        Price = newPrice;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();
        if (quantity > StockQuantity)
            throw new InsufficientStockException(Id, quantity, StockQuantity);

        StockQuantity -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        StockQuantity += quantity;
    }

    // Soft delete: order history keeps referencing the row, but it is hidden from the
    // catalog (query filter) and can no longer be sold.
    public void Delete()
    {
        IsDeleted = true;
        IsActive = false;
    }

    private static void EnsureValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > ProductConstants.NameMaxLength)
            throw new DomainValidationException(
                $"Product name is required and cannot exceed {ProductConstants.NameMaxLength} characters");
    }

    private static void EnsureShopCurrency(Money price)
    {
        if (price.Currency != ShopConstants.Currency)
            throw new CurrencyMismatchException();
    }
}
