namespace OrderPoint.Application.Dtos;

/// <summary>The signed-in user's account.</summary>
/// <param name="Email">The email the user signs in with.</param>
/// <param name="ImageUrl">The public URL of the profile image (for a bartender, the photo the admin set), or `null`
/// if there is none.</param>
/// <param name="FirstName">A bartender's first name, set by the admin; `null` for the admin.</param>
/// <param name="LastName">A bartender's last name, set by the admin; `null` for the admin.</param>
/// <param name="PhoneNumber">A bartender's phone number, set by the admin; `null` for the admin or if there is
/// none.</param>
public sealed record AccountDto(
    string Email,
    string? ImageUrl,
    string? FirstName,
    string? LastName,
    string? PhoneNumber);