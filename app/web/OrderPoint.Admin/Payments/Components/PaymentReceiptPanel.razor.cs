using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Components;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Admin.Payments.Components;

// The selected payment as its till receipt, to print again or to open the order
public sealed partial class PaymentReceiptPanel
{
    [Parameter]
    [EditorRequired]
    public OrderDto? Order { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnDetailsClick { get; set; }

    private Receipt ReceiptComponent { get; set; } = null!;

    private async Task PrintAsync()
    {
        await ReceiptComponent.PrintAsync();
    }

    private async Task OnDetailsClickAsync()
    {
        await OnDetailsClick.InvokeAsync(Order!);
    }
}