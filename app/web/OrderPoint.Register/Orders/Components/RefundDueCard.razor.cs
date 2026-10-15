using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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

    private bool IsCash => Order.PaymentMethod == PaymentMethod.Cash;

    private string MethodIcon => IsCash ? Icons.Material.Filled.Payments : Icons.Material.Filled.CreditCard;

    private string MethodText => Order.PaidByBartender is { } paidBy
        ? $"Paid {(IsCash ? "in cash" : "by card")} to {paidBy.FirstName} {paidBy.LastName}"
        : $"Paid {(IsCash ? "in cash" : "by card")}";

    private string HandBackText => IsCash
        ? $"Hand back {Order.Total.ToString("C", CultureInfo.CurrentCulture)} in cash"
        : $"Refund {Order.Total.ToString("C", CultureInfo.CurrentCulture)} on the terminal";

    private async Task OnHandBackClickAsync()
    {
        await OnHandBack.InvokeAsync(Order);
    }
}