using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Orders.Components;

// The status in the guest's words, its steps with times, and what was ordered
public sealed partial class OrderTrackingDetails
{
    private const string NotReachedStyle = "color: var(--mud-palette-text-disabled)";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderTrackingDto Order { get; set; } = null!;

    // Shows the cancel button while no bartender has taken the order
    [Parameter]
    public EventCallback OnCancelClick { get; set; }

    [Parameter]
    public bool IsCancelling { get; set; }

    private bool CanCancel => Order.Status is OrderStatus.Pending && OnCancelClick.HasDelegate;

    // A cash order is paid at the register, which also takes cards on the bar's terminal
    private string PaymentText => Order.PaymentStatus switch
    {
        PaymentStatus.Paid when Order.PaymentMethod == PaymentMethod.Cash => "Paid in cash",
        PaymentStatus.Paid => "Paid by card",
        PaymentStatus.RefundDue => "Refund at the register",
        PaymentStatus.Refunded when Order.PaymentMethod == PaymentMethod.Card => "Refunded to your card",
        PaymentStatus.Refunded => "Refunded",
        _ when Order.Status is OrderStatus.Declined or OrderStatus.Cancelled => "Nothing to pay",
        _ => "To pay at the register"
    };

    private bool IsToPay => Order.PaymentStatus == PaymentStatus.Unpaid
                            && Order.Status is not (OrderStatus.Declined or OrderStatus.Cancelled);

    private string PaymentIcon => Order.PaymentMethod == PaymentMethod.Card
        ? Icons.Material.Filled.CreditCard
        : Icons.Material.Filled.Payments;

    private string PaymentStyle => IsToPay
        ? "color: var(--mud-palette-warning); font-weight: 600"
        : "color: var(--mud-palette-text-secondary)";

    private IReadOnlyList<(OrderStatus Status, DateTimeOffset? ReachedAtUtc)> StatusSteps =>
        Order.Status switch
        {
            OrderStatus.Declined =>
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Declined, Order.DeclinedAtUtc)
            ],
            OrderStatus.Cancelled =>
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Cancelled, Order.CancelledAtUtc)
            ],
            _ =>
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Accepted, Order.AcceptedAtUtc),
                (OrderStatus.Active, Order.ActivatedAtUtc),
                (OrderStatus.Completed, Order.CompletedAtUtc)
            ]
        };

    private static string GetLineTotalText(OrderItemDto orderItem)
    {
        return (orderItem.Quantity * orderItem.UnitPrice).ToString("C", CultureInfo.CurrentCulture);
    }
}