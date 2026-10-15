using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Payments.Components;

// One payment: how it came in, the order, who took it, its state and the amount
public sealed partial class PaymentRow
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    private PaymentChannel? Channel => Order.GetPaymentChannel();

    private string StatusText => Order.PaymentStatus switch
    {
        PaymentStatus.RefundDue => "Refund due",
        PaymentStatus.Refunded => "Refunded",
        _ => "Paid"
    };

    // A refunded amount is no longer with the bar
    private string? AmountStyle => Order.PaymentStatus == PaymentStatus.Refunded
        ? "text-decoration: line-through; color: var(--mud-palette-text-secondary);"
        : null;
}