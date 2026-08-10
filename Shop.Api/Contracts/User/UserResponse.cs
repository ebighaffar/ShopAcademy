namespace Shop.Api.Contracts.User;

public record UserResponse(
    long Id,
    string? UserName,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTimeOffset CreatedAt);
