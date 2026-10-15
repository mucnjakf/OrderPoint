using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Orders.Dialogs;

// After a payment: what was paid and the change on screen, and the till receipt to print through the browser (a
// receipt printer works as a normal printer)
public sealed partial class ReceiptDialog : IAsyncDisposable
{
    private const string ModulePath = "./Orders/Dialogs/ReceiptDialog.razor.js";

    private const string ChangeStyle = "background-color: rgba(var(--mud-palette-success-rgb), 0.12);";

    private IJSObjectReference? _module;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private ElementReference ReceiptElement { get; set; }

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

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser is already gone, and the module with it
        }
    }

    private async Task PrintAsync()
    {
        _module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);

        await _module.InvokeVoidAsync("printReceipt", ReceiptElement);
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}