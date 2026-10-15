using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Components;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Orders.Dialogs;

// After a payment: what was paid and the change on screen, and the till receipt to print
public sealed partial class ReceiptDialog
{
    private const string ChangeStyle = "background-color: rgba(var(--mud-palette-success-rgb), 0.12);";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private Receipt ReceiptComponent { get; set; } = null!;

    private bool IsRefunded => Order.PaymentStatus is PaymentStatus.RefundDue or PaymentStatus.Refunded;

    private string StatusText => IsRefunded ? "Refunded" : "Paid";

    private string StatusIcon => IsRefunded ? Icons.Material.Filled.AssignmentReturn : Icons.Material.Filled.CheckCircle;

    private Color StatusColor => IsRefunded ? Color.Warning : Color.Success;

    private string PaymentText => Order.PaymentMethod switch
    {
        PaymentMethod.Cash when Order.CashReceived is { } cashReceived =>
            $"Cash · {cashReceived.ToString("C", CultureInfo.CurrentCulture)} received",
        PaymentMethod.Cash => "Cash",
        _ => Order.PaidByBartender is null ? "Card in the customer app" : "Card on the terminal"
    };

    private decimal? ChangeGiven => Order.PaymentMethod == PaymentMethod.Cash && Order.CashReceived is { } received
        ? received - Order.Total
        : null;

    private string PaidAtText => (Order.PaidAtUtc ?? Order.CreatedAtUtc).ToDisplayDateTime(TimeZoneService.TimeZone);

    private async Task PrintAsync()
    {
        await ReceiptComponent.PrintAsync();
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}