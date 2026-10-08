using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

public sealed record AuthTokensDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    string Email,
    UserRole Role);