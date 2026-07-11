using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public record ProductDto(
    long Id,
    string Name,
    string? Description,
    bool IsActive,
    decimal PriceAmount,
    Currency PriceCurrency,
    int Count,
    long CategoryId,
    DateTimeOffset CreatedAt);
