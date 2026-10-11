using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>The tokens of a session.</summary>
/// <param name="AccessToken">The JWT to send as `Authorization: Bearer &lt;token&gt;`.</param>
/// <param name="AccessTokenExpiresAtUtc">When the access token expires (UTC). Refresh shortly before.</param>
/// <param name="RefreshToken">The token for `POST /api/auth/refresh`, valid for 7 days and usable once.</param>
/// <param name="Email">The email of the signed-in user.</param>
/// <param name="Role">The role of the signed-in user.</param>
public sealed record AuthTokensDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    string Email,
    UserRole Role);