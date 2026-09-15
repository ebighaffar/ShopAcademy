namespace Shop.Application.DTOs;

public record RefreshTokenValue(
    string Token,
    string Hash,
    string Family,
    DateTimeOffset ExpiresAt);
