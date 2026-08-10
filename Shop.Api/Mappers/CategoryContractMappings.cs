using Shop.Api.Contracts.Category;
using Shop.Application.DTOs;

namespace Shop.Api.Mappers;

public static class CategoryContractMappings
{
    public static CategoryResponse ToResponse(this CategoryDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Description,
        dto.CreatedAt);

    public static CreateCategoryDto ToDto(this CreateCategoryRequest request) => new(
        request.Name,
        request.Description);

    public static UpdateCategoryDto ToDto(this UpdateCategoryRequest request) => new(
        request.Name,
        request.Description);
}
