using Shop.Domain.Enums;
using Shop.Domain.Exceptions;

namespace Shop.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new InvalidPriceAmountException();

        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        return Currency != other.Currency
            ? throw new CurrencyMismatchException()
            : new Money(Amount + other.Amount, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
}