using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Authentication;

namespace Shop.Infrastructure.Services;

public class RefreshTokenService(IOptionsSnapshot<JwtSettings> options) : IRefreshTokenService
{
    public RefreshTokenValue Create(string? family = null)
    {
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        return new RefreshTokenValue(
            rawToken,
            Hash(rawToken),
            family ?? Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            DateTimeOffset.UtcNow.AddDays(options.Value.RefreshTokenDays));
    }

    public string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
