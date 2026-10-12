using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class LiveOrdersPanelSkeleton
{
    private static readonly OrderStatus[] OpenStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Accepted,
        OrderStatus.Active
    ];

    private static readonly string[][] TodayLabelRows =
    [
        ["Orders", "Completed"],
        ["Revenue", "Average order"]
    ];
}