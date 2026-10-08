using OrderPoint.Admin.Auth.Enumerations;

namespace OrderPoint.Admin.Auth.Dtos;

internal sealed record AuthTokensDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    string Email,
    UserRole Role);