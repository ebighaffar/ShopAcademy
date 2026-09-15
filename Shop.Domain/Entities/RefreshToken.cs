namespace Shop.Domain.Entities;

public class RefreshToken
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public string Family { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public long? ReplacedByTokenId { get; private set; }
    public User User { get; private set; } = null!;
    public RefreshToken? ReplacedByToken { get; private set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    private RefreshToken()
    {
    }

    public RefreshToken(long userId, string tokenHash, string family, DateTimeOffset expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        Family = family;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(long? replacedByTokenId = null)
    {
        if (RevokedAt is not null)
            return;

        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
