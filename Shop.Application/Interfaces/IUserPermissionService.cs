namespace Shop.Application.Interfaces;

public interface IUserPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(long userId,
        CancellationToken cancellationToken = default);
}
