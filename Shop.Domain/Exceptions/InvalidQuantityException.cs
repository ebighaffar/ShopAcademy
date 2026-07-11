namespace Shop.Domain.Exceptions;

public class InvalidQuantityException() : Exception("Invalid quantity. Quantity must be greater than 0")
{
}
