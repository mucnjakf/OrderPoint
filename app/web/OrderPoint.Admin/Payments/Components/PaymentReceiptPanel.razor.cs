using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Components;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Payments.Components;

// The selected payment at a glance: amount, how and when, who took it, what was bought; Print sends its till receipt
public sealed partial class PaymentReceiptPanel
{
    private const string LabelStyle = "color: var(--mud-palette-text-secondary);";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderDto? Order { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnDetailsClick { get; set; }

    private Receipt ReceiptComponent { get; set; } = null!;

    private PaymentChannel? Channel => Order?.GetPaymentChannel();

    private string MethodText => Channel switch
    {
        PaymentChannel.Cash => "Cash at the register",
        PaymentChannel.CardAtRegister => "Card on the bar's terminal",
        _ => "Card in the customer app"
    };

    // A refunded amount is no longer with the bar
    private string AmountStyle => Order?.PaymentStatus == PaymentStatus.Refunded
        ? "font-weight: 600; text-decoration: line-through; color: var(--mud-palette-text-secondary);"
        : "font-weight: 600;";

    private string? RefundText => Order?.PaymentStatus switch
    {
        PaymentStatus.RefundDue => "Due at the register",
        PaymentStatus.Refunded => Order.RefundedAtUtc?.ToDisplayDateTime(TimeZoneService.TimeZone),
        _ => null
    };

    private static string GetLineTotalText(OrderItemDto orderItem)
    {
        return (orderItem.Quantity * orderItem.UnitPrice).ToString("C", CultureInfo.CurrentCulture);
    }

    private async Task PrintAsync()
    {
        await ReceiptComponent.PrintAsync();
    }

    private async Task OnDetailsClickAsync()
    {
        await OnDetailsClick.InvokeAsync(Order!);
    }
}