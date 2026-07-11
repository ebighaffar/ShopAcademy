namespace Shop.Domain.Enums;

public enum OrderStatus : byte
{
    Pending = 1,
    Paid = 2,
    Shipped = 3,
    Cancelled = 4,
    Completed = 5
}
