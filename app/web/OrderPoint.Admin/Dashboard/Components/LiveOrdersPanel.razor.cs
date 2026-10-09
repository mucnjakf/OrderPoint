using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Orders.Enumerations;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class LiveOrdersPanel
{
    private static readonly OrderStatus[] OpenStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Accepted,
        OrderStatus.Active
    ];

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public DashboardLiveDto Live { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    private int GetCount(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => Live.PendingCount,
            OrderStatus.Accepted => Live.AcceptedCount,
            OrderStatus.Active => Live.ActiveCount,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}