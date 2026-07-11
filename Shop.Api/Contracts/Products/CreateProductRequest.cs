using Shop.Domain.Enums;

namespace Shop.Api.Contracts.Products;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal PriceAmount,
    Currency PriceCurrency,
    long CategoryId);
