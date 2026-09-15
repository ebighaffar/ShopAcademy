using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Repositories;
using Shop.Application.Services.Interfaces;
using Shop.Domain.Constants;
using Shop.Domain.Entities;
using Shop.Domain.Exceptions;

namespace Shop.Application.Services.Implementations;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) : IAuthService
{
    public async Task<AuthenticationDto> RegisterAsync(RegisterUserDto dto,
        CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetByUserNameAsync(dto.UserName, cancellationToken) is not null)
            throw new DuplicateUserNameException();

        if (await userRepository.GetByEmailAsync(dto.Email, cancellationToken) is not null)
            throw new DuplicateEmailException();

        var userRole = await roleRepository.GetByNameAsync(Roles.User, cancellationToken)
                       ?? throw new InvalidOperationException(
                           $"The required '{Roles.User}' role has not been seeded.");

        var user = new User(dto.FirstName, dto.LastName, dto.PhoneNumber, dto.UserName, dto.Email);
        user.ChangePasswordHash(passwordHasher.Hash(user, dto.Password));
        user.AssignRole(userRole);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthenticationDto> LoginAsync(LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByLoginWithRolesAsync(dto.Login, cancellationToken);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user, dto.Password))
            throw new AuthenticationFailedException();

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthenticationDto> RefreshAsync(string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenService.Hash(rawRefreshToken);
        var currentToken = await refreshTokenRepository.GetByHashWithUserAsync(
            tokenHash, cancellationToken);

        if (currentToken is null)
            throw new InvalidRefreshTokenException();

        if (currentToken.RevokedAt is not null)
        {
            await RevokeFamilyAsync(currentToken.Family, cancellationToken);
            throw new InvalidRefreshTokenException();
        }

        if (!currentToken.IsActive || !currentToken.User.IsActive)
        {
            currentToken.Revoke();
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            throw new InvalidRefreshTokenException();
        }

        var value = refreshTokenService.Create(currentToken.Family);
        var replacement = new RefreshToken(
            currentToken.UserId,
            value.Hash,
            value.Family,
            value.ExpiresAt);

        await refreshTokenRepository.AddAsync(replacement, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        currentToken.Revoke(replacement.Id);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return CreateResponse(currentToken.User, value);
    }

    public async Task LogoutAsync(long userId, string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var token = await refreshTokenRepository.GetByHashWithUserAsync(
            refreshTokenService.Hash(rawRefreshToken), cancellationToken);

        if (token is null || token.UserId != userId)
            return;

        token.Revoke();
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutAllAsync(long userId, CancellationToken cancellationToken = default)
    {
        var tokens = await refreshTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        foreach (var token in tokens)
            token.Revoke();

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthenticationDto> CreateSessionAsync(User user,
        CancellationToken cancellationToken)
    {
        var value = refreshTokenService.Create();
        var refreshToken = new RefreshToken(user.Id, value.Hash, value.Family, value.ExpiresAt);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return CreateResponse(user, value);
    }

    private AuthenticationDto CreateResponse(User user, RefreshTokenValue refreshToken)
    {
        var roles = user.Roles
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role)
            .ToArray();
        var accessToken = tokenService.CreateAccessToken(user, roles);

        return new AuthenticationDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            ToDto(user, roles));
    }

    private async Task RevokeFamilyAsync(string family, CancellationToken cancellationToken)
    {
        var tokens = await refreshTokenRepository.GetActiveByFamilyAsync(family, cancellationToken);
        foreach (var token in tokens)
            token.Revoke();

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private static UserDto ToDto(User user, IReadOnlyCollection<string> roles) => new(
        user.Id,
        user.UserName,
        user.Email,
        user.FirstName,
        user.LastName,
        user.PhoneNumber,
        user.CreatedAt,
        user.IsActive,
        roles);
}