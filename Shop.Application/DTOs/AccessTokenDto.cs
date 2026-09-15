namespace Shop.Application.DTOs;

public record AccessTokenDto(string Token, DateTimeOffset ExpiresAt);
