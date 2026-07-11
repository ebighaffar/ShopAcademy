using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public record CreateProductDto(
    string Name,
    string? Description,
    decimal PriceAmount,
    Currency PriceCurrency,
    long CategoryId);
