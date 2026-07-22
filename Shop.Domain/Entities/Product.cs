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
    public int Count { get; private set; }
    public long CategoryId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Category? Category { get; private init; }


    private Product()
    {
    }

    public Product(string name, string? description, Money price, int count, long categoryId)
    {
        Name = name;
        Description = description;
        IsActive = true;
        IsDeleted = false;
        Price = price;
        Count = count;
        CategoryId = categoryId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? description, long categoryId)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
    }

    public void ToggleStatus()
    {
        IsActive = !IsActive;
    }

    public void IncreasePrice(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidPriceAmountException();

        Price = new Money(Price.Amount + amount, Price.Currency);
    }
}