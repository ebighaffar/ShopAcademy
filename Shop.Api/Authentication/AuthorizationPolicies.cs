using Microsoft.AspNetCore.Authorization;
using Shop.Domain.Constants;

namespace Shop.Api.Authentication;

public static class AuthorizationPolicies
{
    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(PolicyNames.CustomerAccess,
            policy => policy.RequireRole(Roles.User));

        options.AddPolicy(PolicyNames.ActiveUser,
            policy => policy.RequireClaim(CustomClaimTypes.AccountStatus, ClaimValues.Active));

        options.AddPolicy(PolicyNames.CanReadProducts,
            policy => policy.Requirements.Add(
                new PermissionRequirement(Permissions.Products.Read)));

        options.AddPolicy(PolicyNames.CanManageProducts,
            policy => policy.Requirements.Add(
                new PermissionRequirement(Permissions.Products.Manage)));

        options.AddPolicy(PolicyNames.CanManageCategories,
            policy => policy.Requirements.Add(
                new PermissionRequirement(Permissions.Categories.Manage)));
    }
}
