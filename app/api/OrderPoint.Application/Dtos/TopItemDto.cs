namespace OrderPoint.Application.Dtos;

/// <summary>An item's sales in the period.</summary>
/// <param name="ItemId">The item's id.</param>
/// <param name="Name">The item's name.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="Quantity">The number of pieces sold in completed orders.</param>
/// <param name="Revenue">The revenue of those pieces, in euros.</param>
public sealed record TopItemDto(
    Guid ItemId,
    string Name,
    string? ImageUrl,
    int Quantity,
    decimal Revenue);