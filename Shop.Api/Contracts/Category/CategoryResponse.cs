namespace Shop.Api.Contracts.Category;

public record CategoryResponse(
    long Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
