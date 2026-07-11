using Shop.Application.DTOs;

namespace Shop.Api.Contracts.Categories;

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
