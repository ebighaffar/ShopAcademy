using Shop.Application.DTOs;
using Shop.Domain.Entities;

namespace Shop.Application.Interfaces;

public interface ITokenService
{
    AccessTokenDto CreateAccessToken(User user, IReadOnlyCollection<string> roles);
}
