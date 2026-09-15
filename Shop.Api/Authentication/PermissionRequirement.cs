using Microsoft.AspNetCore.Authorization;

namespace Shop.Api.Authentication;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
