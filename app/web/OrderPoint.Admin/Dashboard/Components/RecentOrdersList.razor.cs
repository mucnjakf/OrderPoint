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

    private async Task OnOrderClickAsync(OrderDto order)
    {
        await OnOrderClick.InvokeAsync(order);
    }
}