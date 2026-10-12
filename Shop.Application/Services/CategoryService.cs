using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Exceptions;

namespace Shop.Application.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    ICacheService cacheService
)
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);
    private const string AllCategoriesCacheKey = "categories:all";
    private static string CategoryCacheKey(long id) => $"categories:{id}";

    public Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => cacheService.GetOrSetAsync<IReadOnlyList<CategoryDto>>(
            AllCategoriesCacheKey,
            async ct => (await categoryRepository.GetAllAsync(ct)).Select(ToDto).ToList(),
            CacheExpiration,
            cancellationToken);

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var cached = await cacheService.GetAsync<CategoryDto>(CategoryCacheKey(id), cancellationToken);
        if (cached is not null)
            return cached;

        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
                       ?? throw new EntityNotFoundException(nameof(Category), id);

        var dto = ToDto(category);
        await cacheService.SetAsync(CategoryCacheKey(id), dto, CacheExpiration, cancellationToken);
        return dto;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category(dto.Name, dto.Description);

        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        await InvalidateAsync(category.Id, cancellationToken);

        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(long id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
                       ?? throw new EntityNotFoundException(nameof(Category), id);

        category.Update(dto.Name, dto.Description);
        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        await InvalidateAsync(id, cancellationToken);

        return ToDto(category);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
                       ?? throw new EntityNotFoundException(nameof(Category), id);

        categoryRepository.Remove(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        await InvalidateAsync(id, cancellationToken);
    }

    // Writes drop both the entity entry and the list entry, since the list is cached
    // as a whole and would otherwise keep serving a stale collection.
    private async Task InvalidateAsync(long id, CancellationToken cancellationToken)
    {
        await cacheService.RemoveAsync(CategoryCacheKey(id), cancellationToken);
        await cacheService.RemoveAsync(AllCategoriesCacheKey, cancellationToken);
    }

    private static CategoryDto ToDto(Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.CreatedAt);
}