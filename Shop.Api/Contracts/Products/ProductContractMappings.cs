using Shop.Application.DTOs;

namespace Shop.Api.Contracts.Products;

public static class ProductContractMappings
{
    public static ProductResponse ToResponse(this ProductDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Description,
        dto.IsActive,
        dto.PriceAmount,
        dto.PriceCurrency,
        dto.Count,
        dto.CategoryId,
        dto.CreatedAt);

    public static CreateProductDto ToDto(this CreateProductRequest request) => new(
        request.Name,
        request.Description,
        request.PriceAmount,
        request.PriceCurrency,
        request.CategoryId);

    public static UpdateProductDto ToDto(this UpdateProductRequest request) => new(
        request.Name,
        request.Description,
        request.CategoryId);
}
