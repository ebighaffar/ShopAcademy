using Shop.Domain.Enums;

namespace Shop.Api.Contracts.V2.Product;

public sealed record ProductPriceResponse(
    decimal Amount,
    Currency Currency);
