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

    private IReadOnlyList<(OrderStatus Status, DateTimeOffset? ReachedAtUtc)> StatusSteps =>
        Order.Status is OrderStatus.Declined
            ?
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Declined, Order.DeclinedAtUtc)
            ]
            :
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Accepted, Order.AcceptedAtUtc),
                (OrderStatus.Active, Order.ActivatedAtUtc),
                (OrderStatus.Completed, Order.CompletedAtUtc)
            ];

    private static string GetLineTotalText(OrderItemDto orderItem)
    {
        return (orderItem.Quantity * orderItem.UnitPrice).ToString("C", CultureInfo.CurrentCulture);
    }
}