using Shop.Application.DTOs;
using Shop.Application.Repository;
using Shop.Domain.Entities;
using Shop.Domain.Exceptions;

namespace Shop.Application.Services;

public class UserService(IUserRepository userRepository)
{
    public async Task<UserDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        var existingUsers = await userRepository.GetAllAsync(cancellationToken);
        if (existingUsers.Any(u => u.UserName == dto.UserName))
            throw new DuplicateUserNameException();

        var user = new User(0, dto.FirstName, dto.LastName, dto.PhoneNumber);
        //user.SetCredentials(dto.UserName, passwordHasher.Hash(dto.Password));

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    private static UserDto ToDto(User user) => new(
        user.Id,
        user.UserName,
        user.FirstName,
        user.LastName,
        user.PhoneNumber,
        user.CreatedAt);
}