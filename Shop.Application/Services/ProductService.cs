using Shop.Application.DTOs;
using Shop.Application.Repository;
using Shop.Domain.Entities;
using Shop.Domain.Exceptions;
using Shop.Domain.ValueObjects;

namespace Shop.Application.Services;

public class ProductService(IProductRepository productRepository)
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new EntityNotFoundException(nameof(Product), id);

        return ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product(dto.Name, dto.Description, new Money(dto.PriceAmount, dto.PriceCurrency), dto.Count, dto.CategoryId);

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        return ToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(long id, UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new EntityNotFoundException(nameof(Product), id);

        product.Update(dto.Name, dto.Description, dto.CategoryId);
        productRepository.Update(product);
        await productRepository.SaveChangesAsync(cancellationToken);

        return ToDto(product);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new EntityNotFoundException(nameof(Product), id);

        productRepository.Update(product);
        await productRepository.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto ToDto(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.IsActive,
        product.Price.Amount,
        product.Price.Currency,
        product.Count,
        product.CategoryId,
        product.CreatedAt);
}