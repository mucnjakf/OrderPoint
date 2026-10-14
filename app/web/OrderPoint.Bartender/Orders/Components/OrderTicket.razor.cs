using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Components;

// Made for a phone: who (table), when, what (one line per item, so nothing is missed) and the total
public sealed partial class OrderTicket
{
    private const string WaitingStyle = "color: var(--mud-palette-warning); font-weight: 600;";

    private const string LongWaitStyle = "color: var(--mud-palette-error); font-weight: 600;";

    private static readonly TimeSpan NoticeableWait = TimeSpan.FromMinutes(5);

    private static readonly TimeSpan LongWait = TimeSpan.FromMinutes(10);

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    [Parameter]
    public bool IsNew { get; set; }

    // The order open in the tablet's details panel
    [Parameter]
    public bool IsSelected { get; set; }

    // Search results can be anyone's orders, so they say who handles them
    [Parameter]
    public bool ShowsBartender { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnClick { get; set; }

    private string? PaperStyle => IsSelected
        ? "background-color: var(--mud-palette-table-hover); border-color: var(--mud-palette-primary);"
        : null;

    private string TimeText => Order.Status switch
    {
        OrderStatus.Completed when Order.CompletedAtUtc is not null =>
            $"Completed {Order.CompletedAtUtc.Value.ToDisplayDateTime(TimeZoneService.TimeZone)}",
        OrderStatus.Declined when Order.DeclinedAtUtc is not null =>
            $"Declined {Order.DeclinedAtUtc.Value.ToDisplayDateTime(TimeZoneService.TimeZone)}",
        OrderStatus.Cancelled when Order.CancelledAtUtc is not null =>
            $"Cancelled {Order.CancelledAtUtc.Value.ToDisplayDateTime(TimeZoneService.TimeZone)}",
        _ => $"Placed {Order.CreatedAtUtc.ToRelativeTime()}"
    };

    private string? BartenderSuffix => ShowsBartender && Order.Bartender is not null
        ? $" · {BartenderText}"
        : null;

    // An open order turns amber, then red, the longer the guest waits, so the oldest ones stand out
    private string? WaitStyle
    {
        get
        {
            if (Order.Status is not (OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.Active))
            {
                return null;
            }

            TimeSpan waited = DateTimeOffset.UtcNow - Order.CreatedAtUtc;

            if (waited >= LongWait)
            {
                return LongWaitStyle;
            }

            return waited >= NoticeableWait ? WaitingStyle : null;
        }
    }

    private string BartenderText => Order.Bartender!.Id == AuthService.UserId
        ? "you"
        : $"{Order.Bartender.FirstName} {Order.Bartender.LastName}";

    private async Task OnTicketClickAsync()
    {
        await OnClick.InvokeAsync(Order);
    }
}