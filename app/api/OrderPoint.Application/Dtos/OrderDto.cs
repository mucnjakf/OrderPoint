using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>An order.</summary>
/// <param name="Id">The id.</param>
/// <param name="Number">The human-readable order number, counting up from 1.</param>
/// <param name="Source">Where it came from: `Guest` (customer app), `Bartender` (Bartender app) or `Register`.
/// </param>
/// <param name="TableCode">The code of the table the order was placed from, e.g. `T11`, or `null` at the
/// counter.</param>
/// <param name="CustomerName">Who to call at the counter, e.g. `Ana`, or `null`.</param>
/// <param name="Note">The customer's note for the bartender, or `null`.</param>
/// <param name="Status">The status: `Pending`, `Accepted`, `Active`, `Completed`, `Declined`, or `Cancelled`.</param>
/// <param name="Total">The sum of quantity times unit price of the lines, in euros.</param>
/// <param name="Items">The order lines.</param>
/// <param name="Bartender">The bartender handling the order, or `null` while nobody has taken it.</param>
/// <param name="TakenByBartender">The bartender who entered it (Bartender app or register), or `null` for a guest's
/// order.</param>
/// <param name="PaidByBartender">The bartender who took the payment at the register, or `null`.</param>
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
/// <param name="CashReceived">What the guest handed over for a cash payment, in euros, or `null`.</param>
/// <param name="CreatedAtUtc">When the order was placed (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
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
    OrderBartenderDto? PaidByBartender,
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
    decimal? CashReceived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);