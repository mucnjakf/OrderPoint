using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Admin.Orders.Enumerations;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Orders.Dialogs;

public sealed partial class OrderDetailsDialog
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private IReadOnlyList<(OrderStatus Status, DateTimeOffset? ReachedAtUtc)> TimelineSteps =>
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

    private static decimal GetSubtotal(OrderItemDto orderItem)
    {
        return orderItem.Quantity * orderItem.UnitPrice;
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}