namespace Shop.Api.Contracts.Auth;

public record CurrentUserResponse(
    long Id,
    string UserName,
    string Email,
    string AccountStatus,
    IReadOnlyCollection<string> Roles);
