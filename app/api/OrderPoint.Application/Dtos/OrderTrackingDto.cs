using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>An order as the guest who placed it follows it: no bartender details.</summary>
/// <param name="Id">The id.</param>
/// <param name="Number">The human-readable order number.</param>
/// <param name="TableCode">The code of the table the order was placed from, e.g. `T11`, or `null` at the
/// counter.</param>
/// <param name="Note">The guest's note for the bartender, or `null`.</param>
/// <param name="Status">The status: `Pending`, `Accepted`, `Active`, `Completed`, `Declined`, or `Cancelled`.</param>
/// <param name="Total">The sum of quantity times unit price of the lines, in euros.</param>
/// <param name="Items">The order lines.</param>
/// <param name="DeclineReason">Why the order was declined, shown to the guest, or `null`.</param>
/// <param name="AcceptedAtUtc">When a bartender accepted the order (UTC), or `null`.</param>
/// <param name="DeclinedAtUtc">When a bartender or the admin declined the order (UTC), or `null`.</param>
/// <param name="ActivatedAtUtc">When the preparation started (UTC), or `null`.</param>
/// <param name="CompletedAtUtc">When the order was completed (UTC), or `null`.</param>
/// <param name="CancelledAtUtc">When the guest cancelled the order (UTC), or `null`.</param>
/// <param name="PaymentStatus">The payment: `Unpaid`, `Paid`, `RefundDue` (cash to hand back), or `Refunded`.</param>
/// <param name="PaymentMethod">How it was paid: `Cash` or `Card`, or `null` while unpaid.</param>
/// <param name="PaidAtUtc">When it was paid (UTC), or `null`.</param>
/// <param name="RefundedAtUtc">When the payment was refunded (UTC), or `null`.</param>
/// <param name="CreatedAtUtc">When the order was placed (UTC).</param>
public sealed record OrderTrackingDto(
    Guid Id,
    int Number,
    string? TableCode,
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
    PaymentStatus PaymentStatus,
    PaymentMethod? PaymentMethod,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? RefundedAtUtc,
    DateTimeOffset CreatedAtUtc);