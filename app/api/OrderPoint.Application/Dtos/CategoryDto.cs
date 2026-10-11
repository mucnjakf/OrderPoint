using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A menu category.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name shown on the menu.</param>
/// <param name="Description">A short description shown on the menu.</param>
/// <param name="Status">The status. Items in an inactive category cannot be ordered.</param>
/// <param name="DisplayOrder">The position on the menu, from 1; lower comes first.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="ItemsCount">The number of items in the category.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    int DisplayOrder,
    string? ImageUrl,
    int ItemsCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);