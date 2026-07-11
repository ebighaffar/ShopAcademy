namespace Shop.Api.Contracts.Users;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string UserName,
    string Password);
