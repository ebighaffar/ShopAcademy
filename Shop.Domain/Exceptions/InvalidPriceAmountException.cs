namespace Shop.Domain.Exceptions;

public class InvalidPriceAmountException() : Exception("Invalid amount of price . price must be greater than 0")
{
}