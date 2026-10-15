using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Dialogs;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Stock.Services;

// Waste and corrections, asked and saved the same way from the stock sheet and the details dialog
internal sealed class StockDialogService(
    IDialogService dialogService,
    ApiService apiService,
    StockApiClient stockApiClient)
{
    private static readonly DialogOptions Options = new()
    {
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };

    internal async Task<bool> RecordWasteAsync(StockItemDto stockItem)
    {
        var parameters = new DialogParameters<RecordStockWasteDialog>
        {
            { dialog => dialog.StockItem, stockItem }
        };

        IDialogReference dialogReference = await dialogService
            .ShowAsync<RecordStockWasteDialog>(string.Empty, parameters, Options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return false;
        }

        var request = (dialogResult.Data as RecordStockWasteRequest)!;

        return await apiService.ExecuteAsync(
            () => stockApiClient.RecordWasteAsync(stockItem.Id, request),
            $"Waste of {stockItem.Unit.FormatAmount(request.Amount)} recorded for {stockItem.Name}");
    }

    internal async Task<bool> CorrectAsync(StockItemDto stockItem)
    {
        var parameters = new DialogParameters<CorrectStockDialog>
        {
            { dialog => dialog.StockItem, stockItem }
        };

        IDialogReference dialogReference = await dialogService
            .ShowAsync<CorrectStockDialog>(string.Empty, parameters, Options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return false;
        }

        var request = (dialogResult.Data as CorrectStockRequest)!;

        return await apiService.ExecuteAsync(
            () => stockApiClient.CorrectStockAsync(stockItem.Id, request),
            $"{stockItem.Name} corrected to {stockItem.GetPacksText(request.Quantity)}");
    }
}