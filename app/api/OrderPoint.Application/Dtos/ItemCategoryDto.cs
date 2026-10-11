using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>The category of an item.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="Status">The status. Items in an inactive category cannot be ordered.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record ItemCategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    string? ImageUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);