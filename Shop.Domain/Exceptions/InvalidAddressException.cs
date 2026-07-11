namespace Shop.Domain.Exceptions;

public class InvalidAddressException() : Exception("Invalid address. Raw address and postal code are required")
{
}
