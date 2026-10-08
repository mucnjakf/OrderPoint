using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Orders.Enumerations;

namespace OrderPoint.Admin.Orders.Components;

public sealed partial class OrderStatusPipeline
{
    private static readonly OrderStatus[] FlowStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Accepted,
        OrderStatus.Active,
        OrderStatus.Completed
    ];

    [Parameter]
    [EditorRequired]
    public IReadOnlyDictionary<OrderStatus, int> StatusCounts { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public IReadOnlySet<OrderStatus> LoadingStatuses { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderStatus? SelectedStatus { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderStatus> OnStatusClick { get; set; }

    private int GetCount(OrderStatus status)
    {
        return StatusCounts.GetValueOrDefault(status);
    }

    private bool IsLoading(OrderStatus status)
    {
        return LoadingStatuses.Contains(status);
    }

    private bool IsSelected(OrderStatus status)
    {
        return SelectedStatus == status;
    }
}