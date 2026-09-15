using System.Globalization;
using System.Security.Claims;
using Shop.Domain.Exceptions;

namespace Shop.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub");
        if (long.TryParse(subject, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
            return userId;

        throw new AuthenticationFailedException();
    }
}
