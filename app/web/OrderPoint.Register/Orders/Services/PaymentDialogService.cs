using MudBlazor;
using OrderPoint.Register.Orders.Dialogs;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Register.Orders.Services;

// The two ways to pay at the register, and the receipt afterwards; the sale and To pay use the same dialogs
internal sealed class PaymentDialogService(IDialogService dialogService)
{
    private static readonly DialogOptions PaymentDialogOptions = new()
    {
        MaxWidth = MaxWidth.ExtraSmall,
        FullWidth = true
    };

    // What the guest handed over, or null when the bartender went back
    internal async Task<decimal?> AskForCashAsync(decimal total)
    {
        var parameters = new DialogParameters<PayCashDialog>
        {
            { dialog => dialog.Total, total }
        };

        IDialogReference dialogReference = await dialogService
            .ShowAsync<PayCashDialog>(string.Empty, parameters, PaymentDialogOptions);

        DialogResult dialogResult = (await dialogReference.Result)!;

        return dialogResult.Canceled ? null : (decimal)dialogResult.Data!;
    }

    // Whether the bar's card terminal approved the payment
    internal async Task<bool> ConfirmCardAsync(decimal total)
    {
        var parameters = new DialogParameters<PayCardDialog>
        {
            { dialog => dialog.Total, total }
        };

        IDialogReference dialogReference = await dialogService
            .ShowAsync<PayCardDialog>(string.Empty, parameters, PaymentDialogOptions);

        DialogResult dialogResult = (await dialogReference.Result)!;

        return !dialogResult.Canceled;
    }

    internal async Task ShowReceiptAsync(OrderDto order)
    {
        var parameters = new DialogParameters<ReceiptDialog>
        {
            { dialog => dialog.Order, order }
        };

        IDialogReference dialogReference = await dialogService
            .ShowAsync<ReceiptDialog>(string.Empty, parameters, PaymentDialogOptions);

        await dialogReference.Result;
    }
}