using Shop.Application.DTOs;

namespace Shop.Api.Contracts.Users;

public static class UserContractMappings
{
    public static UserResponse ToResponse(this UserDto dto) => new(
        dto.Id,
        dto.UserName,
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber,
        dto.CreatedAt);

    public static RegisterUserDto ToDto(this RegisterUserRequest request) => new(
        request.FirstName,
        request.LastName,
        request.PhoneNumber,
        request.UserName,
        request.Password);
}
