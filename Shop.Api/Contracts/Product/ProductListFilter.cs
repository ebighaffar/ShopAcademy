namespace Shop.Api.Contracts.Product;

public record ProductListFilter(
    string? SearchPhase,
    long? CategoryId,
    long? MinCount,
    long? MaxPrice
);