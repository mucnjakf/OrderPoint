namespace OrderPoint.Application.Dtos;

/// <summary>A bartender's results in the period.</summary>
/// <param name="BartenderId">The bartender's id.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="ImageUrl">The public URL of the image, or `null` if there is none.</param>
/// <param name="HandledOrdersCount">The orders assigned to the bartender in the period, in any status.</param>
/// <param name="CompletedOrdersCount">The completed orders.</param>
/// <param name="DeclinedOrdersCount">The declined orders.</param>
/// <param name="Revenue">The revenue of the completed orders, in euros.</param>
public sealed record BartenderLeaderboardEntryDto(
    Guid BartenderId,
    string FirstName,
    string LastName,
    string? ImageUrl,
    int HandledOrdersCount,
    int CompletedOrdersCount,
    int DeclinedOrdersCount,
    decimal Revenue);