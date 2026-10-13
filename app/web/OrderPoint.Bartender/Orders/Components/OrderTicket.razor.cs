using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Components;

// Made for a phone: who (table), when, what (one line per item, so nothing is missed) and the total
public sealed partial class OrderTicket
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    [Parameter]
    public bool IsNew { get; set; }

    private string TimeText => Order.Status switch
    {
        OrderStatus.Completed when Order.CompletedAtUtc is not null =>
            $"Completed {Order.CompletedAtUtc.Value.ToDisplayDateTime(TimeZoneService.TimeZone)}",
        OrderStatus.Declined when Order.DeclinedAtUtc is not null =>
            $"Declined {Order.DeclinedAtUtc.Value.ToDisplayDateTime(TimeZoneService.TimeZone)}",
        _ => $"Placed {Order.CreatedAtUtc.ToRelativeTime()}"
    };
}