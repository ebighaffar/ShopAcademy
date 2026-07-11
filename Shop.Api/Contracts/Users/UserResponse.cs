namespace Shop.Api.Contracts.Users;

public record UserResponse(
    long Id,
    string? UserName,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTimeOffset CreatedAt);
