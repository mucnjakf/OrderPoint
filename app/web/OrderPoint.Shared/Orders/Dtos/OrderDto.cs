using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Shared.Orders.Dtos;

public sealed record OrderDto(
    Guid Id,
    int Number,
    OrderSource Source,
    string? TableCode,
    string? CustomerName,
    string? Note,
    OrderStatus Status,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    OrderBartenderDto? Bartender,
    OrderBartenderDto? TakenByBartender,
    string? DeclineReason,
    DateTimeOffset? AcceptedAtUtc,
    DateTimeOffset? DeclinedAtUtc,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    PaymentStatus PaymentStatus,
    PaymentMethod? PaymentMethod,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? RefundedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);