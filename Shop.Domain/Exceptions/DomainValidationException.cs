namespace Shop.Domain.Exceptions;

public class DomainValidationException(string message) : Exception(message)
{
}
