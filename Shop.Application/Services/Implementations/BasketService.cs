using Shop.Application.DTOs.Basket;
using Shop.Application.Repositories;
using Shop.Application.Services.Interfaces;

namespace Shop.Application.Services.Implementations;

public class BasketService(IBasketRepository basketRepository) : IBasketService
{
    public async Task<BasketDetailDto> GetUserBasketAsync(long userId, CancellationToken cancellation)
    {
        var basket = await basketRepository.GetByUserIdAsync(userId, cancellation);
        return new BasketDetailDto(
            basket?.Items.Count ?? 0
        );
    }
}
