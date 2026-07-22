using Shop.Domain.ValueObjects;

namespace Shop.Domain.Entities;

public class User
{
    public long Id { get; private set; }
    public string? UserName { get; private set; }
    public string? PasswordHash { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public AddressValueObject? Address { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Basket? Basket { get; private set; }

    private User()
    {
    }

    public User(long id, string firstName, string lastName, string phoneNumber)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCredentials(string userName, string passwordHash)
    {
        UserName = userName;
        PasswordHash = passwordHash;
    }

    public void SetAddress(AddressValueObject address)
    {
        Address = address;
    }

    public Basket GetOrCreateBasket(long basketId)
    {
        Basket ??= new Basket(basketId, Id);
        return Basket;
    }
}