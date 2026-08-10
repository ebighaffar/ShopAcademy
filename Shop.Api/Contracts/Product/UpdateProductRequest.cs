namespace Shop.Api.Contracts.Product;

public record UpdateProductRequest(
    string Name,
    string? Description,
    long CategoryId);
