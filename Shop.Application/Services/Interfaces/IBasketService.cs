using Shop.Application.DTOs.Basket;

namespace Shop.Application.Services.Interfaces;

public interface IBasketService
{
    public Task<BasketDetailDto> GetUserBasketAsync(long userId, CancellationToken cancellation);
}