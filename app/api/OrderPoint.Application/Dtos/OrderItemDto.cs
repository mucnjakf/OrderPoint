namespace OrderPoint.Application.Dtos;

/// <summary>An order line.</summary>
/// <param name="Id">The id of the line.</param>
/// <param name="ItemId">The ordered item's id.</param>
/// <param name="Name">The item's name.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="Quantity">How many pieces.</param>
/// <param name="UnitPrice">The price of one piece when the order was placed, in euros.</param>
public sealed record OrderItemDto(
    Guid Id,
    Guid ItemId,
    string Name,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice);