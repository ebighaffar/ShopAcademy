using Shop.Domain.Enums;

namespace Shop.Api.Contracts.Products;

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
