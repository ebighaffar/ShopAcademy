namespace Shop.Api.Contracts.Categories;

public record CategoryResponse(
    long Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
