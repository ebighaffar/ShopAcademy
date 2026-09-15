using Shop.Api.Contracts.Auth;
using Shop.Api.Contracts.User;
using Shop.Application.DTOs;

namespace Shop.Api.Mappers;

public static class UserContractMappings
{
    public static UserResponse ToResponse(this UserDto dto) => new(
        dto.Id,
        dto.UserName,
        dto.Email,
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber,
        dto.CreatedAt,
        dto.IsActive,
        dto.Roles);

    public static RegisterUserDto ToDto(this RegisterUserRequest request) => new(
        request.FirstName,
        request.LastName,
        request.PhoneNumber,
        request.UserName,
        request.Email,
        request.Password);

    public static LoginDto ToDto(this LoginRequest request)
        => new(request.Login, request.Password);

    public static AuthenticationResponse ToResponse(this AuthenticationDto dto) => new(
        dto.AccessToken,
        dto.AccessTokenExpiresAt,
        dto.RefreshToken,
        dto.RefreshTokenExpiresAt,
        dto.User.ToResponse());
}
