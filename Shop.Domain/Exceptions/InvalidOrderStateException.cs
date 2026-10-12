using Shop.Domain.Enums;

namespace Shop.Domain.Exceptions;

public class InvalidOrderStateException(OrderStatus current, string action)
    : Exception($"Cannot {action} an order in '{current}' status")
{
}
