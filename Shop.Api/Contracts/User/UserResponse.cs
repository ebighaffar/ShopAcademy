namespace Shop.Api.Contracts.User;

public record UserResponse(
    long Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTimeOffset CreatedAt,
    bool IsActive,
    IReadOnlyCollection<string> Roles);
