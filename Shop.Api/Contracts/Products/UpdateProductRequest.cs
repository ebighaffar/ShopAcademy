namespace Shop.Api.Contracts.Products;

public record UpdateProductRequest(
    string Name,
    string? Description,
    long CategoryId);
