namespace Shop.Application.DTOs;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string UserName,
    string Email,
    string Password);
