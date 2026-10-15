using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A menu item.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name shown on the menu.</param>
/// <param name="Description">The description shown on the menu.</param>
/// <param name="Portion">The portion size, e.g. `0.5` for half a litre or `1` for one piece.</param>
/// <param name="Price">The current price in euros.</param>
/// <param name="Status">The item's own status. It can be ordered only while its category is active too.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="Category">The category the item belongs to.</param>
/// <param name="OrdersCount">The number of orders that contain the item.</param>
/// <param name="Cost">What one serving costs in stock, in euros, or `null` without a recipe.</param>
/// <param name="IsSoldOut">Whether an ingredient has less than one serving left; a sold-out item can't be
/// ordered.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record ItemDto(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    ItemStatus Status,
    string? ImageUrl,
    ItemCategoryDto Category,
    int OrdersCount,
    decimal? Cost,
    bool IsSoldOut,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);