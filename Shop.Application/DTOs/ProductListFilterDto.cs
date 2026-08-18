namespace Shop.Application.DTOs;

public record ProductListFilterDto(
    string? SearchPhase,
    long? CategoryId,
    long? MinCount,
    long? MaxPrice
);