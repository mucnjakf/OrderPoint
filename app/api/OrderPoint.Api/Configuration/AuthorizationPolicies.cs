using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Api.Configuration;

internal static class AuthorizationPolicies
{
    internal const string Admin = nameof(UserRole.Admin);
}