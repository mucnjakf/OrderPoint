using System.Globalization;
using Microsoft.AspNetCore.Components;
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

    // Guests pay by card in this app
    private string PaymentText => Order.PaymentStatus switch
    {
        PaymentStatus.Paid => "Paid by card",
        PaymentStatus.RefundDue or PaymentStatus.Refunded => "Refunded to your card",
        _ => "Not paid yet"
    };

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