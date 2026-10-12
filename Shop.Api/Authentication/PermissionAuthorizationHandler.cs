using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Shop.Api.Extensions;
using Shop.Application.Interfaces;

namespace Shop.Api.Authentication;

public class PermissionAuthorizationHandler(
    IUserPermissionService permissionService
) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.GetUserId();
        var permissions = await permissionService.GetPermissionsAsync(userId);
        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}