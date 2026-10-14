using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Orders.Components;

public sealed partial class MyOrderCard
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderTrackingDto Order { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderTrackingDto> OnClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderTrackingDto> OnOrderAgain { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderTrackingDto> OnCancel { get; set; }

    [Parameter]
    public bool IsCancelling { get; set; }

    // Only while no bartender has taken the order
    private bool CanCancel => Order.Status is OrderStatus.Pending;

    private string LinesText => string.Join(", ", Order.Items.Select(item => $"{item.Quantity}× {item.Name}"));

    private async Task OnCardClickAsync()
    {
        await OnClick.InvokeAsync(Order);
    }

    private async Task OnCancelClickAsync()
    {
        await OnCancel.InvokeAsync(Order);
    }

    private async Task OnOrderAgainClickAsync()
    {
        await OnOrderAgain.InvokeAsync(Order);
    }
}