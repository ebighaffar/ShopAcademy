using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Authentication;

public class JwtTokenService(IOptionsSnapshot<JwtSettings> options) : ITokenService
{
    public AccessTokenDto CreateAccessToken(User user, IReadOnlyCollection<string> roles)
    {
        var settings = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(now.UtcDateTime).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Name, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(CustomClaimTypes.AccountStatus, user.IsActive ? ClaimValues.Active : ClaimValues.NotActive)
        };

        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new AccessTokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}