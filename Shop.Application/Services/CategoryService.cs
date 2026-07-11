using Shop.Application.DTOs;
using Shop.Application.Repository;
using Shop.Domain.Entities;
using Shop.Domain.Exceptions;

namespace Shop.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category(0, dto.Name);
        category.Update(dto.Name, dto.Description);

        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(long id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        category.Update(dto.Name, dto.Description);
        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        categoryRepository.Remove(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);
    }

    private static CategoryDto ToDto(Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.CreatedAt);
}
