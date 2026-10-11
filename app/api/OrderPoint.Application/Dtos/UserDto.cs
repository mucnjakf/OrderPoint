using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A user account (admin or bartender login).</summary>
/// <param name="Id">The id.</param>
/// <param name="Email">The email the user signs in with.</param>
/// <param name="Role">The role.</param>
/// <param name="MustChangePassword">Whether the user still has a temporary password.</param>
/// <param name="ImageUrl">The public URL of the profile image, or `null`.</param>
public sealed record UserDto(
    Guid Id,
    string Email,
    UserRole Role,
    bool MustChangePassword,
    string? ImageUrl);