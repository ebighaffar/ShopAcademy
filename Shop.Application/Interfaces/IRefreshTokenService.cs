using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

public interface IRefreshTokenService
{
    RefreshTokenValue Create(string? family = null);
    string Hash(string token);
}
