using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Customer.Orders.Dtos;

public sealed record OrderTrackingDto(
    Guid Id,
    int Number,
    string TableCode,
    string? Note,
    OrderStatus Status,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    string? DeclineReason,
    DateTimeOffset? AcceptedAtUtc,
    DateTimeOffset? DeclinedAtUtc,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset CreatedAtUtc);