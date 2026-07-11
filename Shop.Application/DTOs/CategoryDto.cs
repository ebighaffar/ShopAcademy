namespace Shop.Application.DTOs;

public record CategoryDto(
    long Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
