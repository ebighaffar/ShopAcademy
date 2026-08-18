using Shop.Domain.Enums;

namespace Shop.Api.Contracts.V2.Product;

public sealed record ProductResponse(
    long Id,
    string Name,
    string? Description,
    bool IsActive,
    ProductPriceResponse Price,
    int StockQuantity,
    long CategoryId,
    DateTimeOffset CreatedAt);

public sealed record ProductPriceResponse(
    decimal Amount,
    Currency Currency);
