namespace Shop.Application.DTOs;

public record UserDto(
    long Id,
    string? UserName,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTimeOffset CreatedAt
);