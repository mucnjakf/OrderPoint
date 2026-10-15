using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Register.Orders.Components;

// An order taken in the Bartender app, waiting for the guest to pay at the register
public sealed partial class UnpaidOrderCard
{
    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public bool IsPaying { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnPayInCash { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnPayByCard { get; set; }

    private async Task OnPayInCashClickAsync()
    {
        await OnPayInCash.InvokeAsync(Order);
    }

    private async Task OnPayByCardClickAsync()
    {
        await OnPayByCard.InvokeAsync(Order);
    }
}