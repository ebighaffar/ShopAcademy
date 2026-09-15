namespace Shop.Api.Contracts.User;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string UserName,
    string Email,
    string Password);
