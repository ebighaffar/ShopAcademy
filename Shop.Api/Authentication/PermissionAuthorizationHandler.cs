using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Shop.Application.Interfaces;

namespace Shop.Api.Authentication;

public class PermissionAuthorizationHandler(IUserPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var subject = context.User.FindFirst("sub")?.Value;
        if (!long.TryParse(subject, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
            return;

        var permissions = await permissionService.GetPermissionsAsync(userId);
        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
