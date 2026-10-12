namespace Shop.Domain.Exceptions;

public class InvalidPaymentException(string message) : Exception(message)
{
}
