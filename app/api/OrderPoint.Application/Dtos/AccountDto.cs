namespace OrderPoint.Application.Dtos;

/// <summary>The signed-in admin's account.</summary>
/// <param name="Email">The email the admin signs in with.</param>
/// <param name="ImageUrl">The public URL of the profile image, or `null` if there is none.</param>
public sealed record AccountDto(string Email, string? ImageUrl);