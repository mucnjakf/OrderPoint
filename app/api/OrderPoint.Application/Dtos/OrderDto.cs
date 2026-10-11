using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>An order.</summary>
/// <param name="Id">The id.</param>
/// <param name="Number">The human-readable order number, counting up from 1.</param>
/// <param name="TableCode">The code of the table the order was placed from, e.g. `T11`.</param>
/// <param name="Note">The customer's note for the bartender, or `null`.</param>
/// <param name="Status">The status: `Pending`, `Accepted`, `Active`, `Completed`, or `Declined`.</param>
/// <param name="Total">The sum of quantity times unit price of the lines, in euros.</param>
/// <param name="Items">The order lines.</param>
/// <param name="Bartender">The bartender handling the order, or `null` while nobody has taken it.</param>
/// <param name="AcceptedAtUtc">When a bartender accepted the order (UTC), or `null`.</param>
/// <param name="DeclinedAtUtc">When a bartender declined the order (UTC), or `null`.</param>
/// <param name="ActivatedAtUtc">When the preparation started (UTC), or `null`.</param>
/// <param name="CompletedAtUtc">When the order was completed (UTC), or `null`.</param>
/// <param name="CreatedAtUtc">When the order was placed (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record OrderDto(
    Guid Id,
    int Number,
    string TableCode,
    string? Note,
    OrderStatus Status,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    OrderBartenderDto? Bartender,
    DateTimeOffset? AcceptedAtUtc,
    DateTimeOffset? DeclinedAtUtc,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);