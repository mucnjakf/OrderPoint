using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Orders.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class RecentOrdersList
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<OrderDto> Orders { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnOrderClick { get; set; }

    private Guid? HoveredOrderId { get; set; }

    private string? GetRowStyle(OrderDto order)
    {
        return order.Id == HoveredOrderId ? "background-color: var(--mud-palette-table-hover);" : null;
    }

    private async Task OnOrderClickAsync(OrderDto order)
    {
        await OnOrderClick.InvokeAsync(order);
    }
}