using Shop.Api.Contracts.V2.Product;
using Shop.Application.DTOs;

namespace Shop.Api.Mappers.V2;

public static class ProductContractMappings
{
    public static ProductResponse ToV2Response(this ProductDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Description,
        dto.IsActive,
        new ProductPriceResponse(dto.PriceAmount, dto.PriceCurrency),
        dto.Count,
        dto.CategoryId,
        dto.CreatedAt);
}
