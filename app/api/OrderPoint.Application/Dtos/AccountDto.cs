namespace OrderPoint.Application.Dtos;

/// <summary>The signed-in user's account.</summary>
/// <param name="Email">The email the user signs in with.</param>
/// <param name="ImageUrl">The public URL of the profile image (for a bartender, the photo the admin set), or `null`
/// if there is none.</param>
public sealed record AccountDto(string Email, string? ImageUrl);