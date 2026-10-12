using Shop.Domain.Enums;

namespace Shop.Domain.Constants;

public static class ShopConstants
{
    // The shop trades in a single currency; prices in any other currency are rejected.
    public const Currency Currency = Enums.Currency.IRR;
}
