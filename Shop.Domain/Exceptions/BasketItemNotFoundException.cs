namespace Shop.Domain.Exceptions;

public class BasketItemNotFoundException(long productId)
    : Exception($"Product '{productId}' is not in the basket")
{
}
