namespace Shop.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string Key { get; init; } = null!;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
}
