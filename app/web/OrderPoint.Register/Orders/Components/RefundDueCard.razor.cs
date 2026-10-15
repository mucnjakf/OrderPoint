using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Register.Orders.Components;

// A paid order the bar declined or the guest cancelled: its money goes back the way it came
public sealed partial class RefundDueCard
{
    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public bool IsRefunding { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnHandBack { get; set; }

    private string CaptionText => Order.PaymentMethod == PaymentMethod.Cash ? "Paid in cash" : "Paid by card";

    private string HandBackText => Order.PaymentMethod == PaymentMethod.Cash
        ? $"Hand back {Order.Total.ToString("C", CultureInfo.CurrentCulture)} in cash"
        : $"Refund {Order.Total.ToString("C", CultureInfo.CurrentCulture)} on the terminal";

    private async Task OnHandBackClickAsync()
    {
        await OnHandBack.InvokeAsync(Order);
    }
}