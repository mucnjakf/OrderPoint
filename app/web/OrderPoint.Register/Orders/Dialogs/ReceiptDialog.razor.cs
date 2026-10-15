using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Register.Orders.Dialogs;

// The receipt after a payment, printed through the browser (a receipt printer works as a normal printer)
public sealed partial class ReceiptDialog : IAsyncDisposable
{
    private const string ModulePath = "./Orders/Dialogs/ReceiptDialog.razor.js";

    private IJSObjectReference? _module;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private ElementReference ReceiptElement { get; set; }

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