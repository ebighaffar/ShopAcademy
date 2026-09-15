using Shop.Application.DTOs;

namespace Shop.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthenticationDto> RegisterAsync(RegisterUserDto dto,
        CancellationToken cancellationToken = default);

    Task<AuthenticationDto> LoginAsync(LoginDto dto,
        CancellationToken cancellationToken = default);

    Task<AuthenticationDto> RefreshAsync(string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(long userId, string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAllAsync(long userId, CancellationToken cancellationToken = default);
}
