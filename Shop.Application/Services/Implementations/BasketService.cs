using Shop.Application.DTOs.Basket;
using Shop.Application.Repositories;
using Shop.Application.Services.Interfaces;

namespace Shop.Application.Services.Implementations;

public class BasketService(IUserRepository userRepository) : IBasketService
{
    public async Task<BasketDetailDto> GetUserBasketAsync(long userId, CancellationToken cancellation)
    {
        var userIncludingBasket = await userRepository.GetByIdIncludeBasketAsync(userId, cancellation);
        return new BasketDetailDto(
            userIncludingBasket?.Basket?.Items.Count ?? 0
        );
    }
}