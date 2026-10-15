namespace OrderPoint.Shared.Menu.Dtos;

public sealed record MenuCategoryDto(
    Guid Id,
    string Name,
    string Description,
    string? ImageUrl,
    IReadOnlyList<MenuItemDto> Items);