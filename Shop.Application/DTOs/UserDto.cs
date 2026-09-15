namespace Shop.Application.DTOs;

public record UserDto(
    long Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTimeOffset CreatedAt,
    bool IsActive,
    IReadOnlyCollection<string> Roles);
