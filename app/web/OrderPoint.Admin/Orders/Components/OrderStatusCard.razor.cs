using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Orders.Enumerations;

namespace OrderPoint.Admin.Orders.Components;

public sealed partial class OrderStatusCard
{
    private const int CardHeight = 150;

    [Parameter]
    [EditorRequired]
    public OrderStatus Status { get; set; }

    [Parameter]
    [EditorRequired]
    public int Count { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsSelected { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderStatus> OnClick { get; set; }

    private async Task OnCardClickAsync()
    {
        await OnClick.InvokeAsync(Status);
    }
}