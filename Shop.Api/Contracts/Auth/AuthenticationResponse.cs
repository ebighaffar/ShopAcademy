using Shop.Api.Contracts.User;

namespace Shop.Api.Contracts.Auth;

public record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    UserResponse User);
