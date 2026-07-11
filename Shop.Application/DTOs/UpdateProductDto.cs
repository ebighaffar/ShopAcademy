namespace Shop.Application.DTOs;

public record UpdateProductDto(
    string Name,
    string? Description,
    long CategoryId);
