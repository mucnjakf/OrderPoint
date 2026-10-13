namespace OrderPoint.Application.Dtos;

/// <summary>An item a guest can order.</summary>
/// <param name="Id">The id, sent as `itemId` when placing an order.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="Portion">The portion, e.g. `0.5`.</param>
/// <param name="Price">The price in euros.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
public sealed record MenuItemDto(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl);