namespace OrderPoint.Application.Dtos;

/// <summary>The bartender handling an order.</summary>
/// <param name="Id">The bartender's id.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Email">The email the bartender signs in with.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
public sealed record OrderBartenderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? ImageUrl);