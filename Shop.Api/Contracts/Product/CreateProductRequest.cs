using Shop.Domain.Enums;

namespace Shop.Api.Contracts.Product;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal PriceAmount,
    Currency PriceCurrency,
    int Count,
    long CategoryId);
