using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace OrderPoint.Api.Extensions;

internal static class ClaimsPrincipalExtensions
{
    // The JWT handler maps "sub" to ClaimTypes.NameIdentifier; endpoints that call this require authorization
    internal static Guid GetUserId(this ClaimsPrincipal user)
    {
        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? throw new InvalidOperationException("The access token has no user id");

        return Guid.Parse(userId);
    }
}