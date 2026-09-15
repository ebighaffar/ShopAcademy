using Shop.Domain.Entities;

namespace Shop.Application.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveByFamilyAsync(string family,
        CancellationToken cancellationToken = default);
}
