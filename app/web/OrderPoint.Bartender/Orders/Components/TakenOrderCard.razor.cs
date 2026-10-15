using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Bartender.Orders.Components;

// An order this bartender took: where it stands at the bar and whether the guest paid at the register
public sealed partial class TakenOrderCard
{
    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public bool IsWithdrawing { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnWithdraw { get; set; }

    // Until the bar accepts it
    private bool CanWithdraw => Order.Status == OrderStatus.Pending;

    private bool IsClosed => Order.Status is OrderStatus.Declined or OrderStatus.Cancelled;

    private string PaymentText => Order.PaymentStatus switch
    {
        PaymentStatus.Unpaid when IsClosed => "Nothing to pay",
        PaymentStatus.Unpaid => "To pay at the register",
        PaymentStatus.Paid => Order.PaymentMethod == PaymentMethod.Cash ? "Paid in cash" : "Paid by card",
        PaymentStatus.RefundDue => "Refund due at the register",
        _ => "Refunded"
    };

    private Color PaymentColor => Order.PaymentStatus switch
    {
        PaymentStatus.Unpaid when IsClosed => Color.Default,
        PaymentStatus.Unpaid or PaymentStatus.RefundDue => Color.Warning,
        PaymentStatus.Paid => Color.Success,
        _ => Color.Default
    };

    private async Task OnWithdrawClickAsync()
    {
        await OnWithdraw.InvokeAsync(Order);
    }
}