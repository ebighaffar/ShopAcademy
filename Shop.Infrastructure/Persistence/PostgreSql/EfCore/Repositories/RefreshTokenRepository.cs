using Microsoft.EntityFrameworkCore;
using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class RefreshTokenRepository(ShopDbContext context)
    : RepositoryBase<RefreshToken>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash,
        CancellationToken cancellationToken = default)
        => Context.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash && token.RevokedAt == null, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
        => await Context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null &&
                            token.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByFamilyAsync(string family,
        CancellationToken cancellationToken = default)
        => await Context.RefreshTokens
            .Where(token => token.Family == family && token.RevokedAt == null &&
                            token.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
}