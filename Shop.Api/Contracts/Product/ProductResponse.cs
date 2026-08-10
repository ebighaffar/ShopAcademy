using Shop.Domain.Enums;

namespace Shop.Api.Contracts.Product;

public record ProductResponse(
    long Id,
    string Name,
    string? Description,
    bool IsActive,
    decimal PriceAmount,
    Currency PriceCurrency,
    int Count,
    long CategoryId,
    DateTimeOffset CreatedAt);
