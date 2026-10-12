namespace Shop.Domain.Exceptions;

public class ProductNotAvailableException(long productId)
    : Exception($"Product '{productId}' is not available for sale")
{
}
