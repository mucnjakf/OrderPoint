namespace OrderPoint.Application.Dtos;

/// <summary>A category of the guests' menu.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="Items">The category's available items, by name.</param>
public sealed record MenuCategoryDto(
    Guid Id,
    string Name,
    string Description,
    string? ImageUrl,
    IReadOnlyList<MenuItemDto> Items);