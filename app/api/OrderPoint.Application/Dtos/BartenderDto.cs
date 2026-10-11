using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A bartender.</summary>
/// <param name="Id">The id; also the id of the bartender's login.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Email">The email the bartender signs in with.</param>
/// <param name="PhoneNumber">The phone number, or `null`.</param>
/// <param name="Status">The status. Inactive bartenders cannot sign in.</param>
/// <param name="Notes">Internal notes, or `null`.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="OrdersCount">The number of orders the bartender handled.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record BartenderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes,
    string? ImageUrl,
    int OrdersCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);