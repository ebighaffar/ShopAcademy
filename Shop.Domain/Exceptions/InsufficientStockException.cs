namespace Shop.Domain.Exceptions;

public class InsufficientStockException(long productId, int requested, int available)
    : Exception($"Product '{productId}' has only {available} item(s) in stock, but {requested} requested")
{
}
