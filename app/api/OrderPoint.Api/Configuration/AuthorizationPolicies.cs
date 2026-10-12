using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Api.Configuration;

internal static class AuthorizationPolicies
{
    internal const string Admin = nameof(UserRole.Admin);

    internal const string Bartender = nameof(UserRole.Bartender);

    internal const string AdminOrBartender = nameof(AdminOrBartender);
}