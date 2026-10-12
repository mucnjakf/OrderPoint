using OrderPoint.Shared.Auth.Enumerations;

namespace OrderPoint.Shared.Auth.Dtos;

internal sealed record AuthTokensDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    string Email,
    UserRole Role);