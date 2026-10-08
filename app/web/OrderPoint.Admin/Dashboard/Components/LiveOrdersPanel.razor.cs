using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Orders.Enumerations;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class LiveOrdersPanel
{
    private static readonly OrderStatus[] OpenStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Accepted,
        OrderStatus.Active
    ];

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