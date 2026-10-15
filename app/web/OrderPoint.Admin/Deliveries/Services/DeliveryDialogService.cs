using System.Globalization;
using MudBlazor;
using OrderPoint.Admin.Deliveries.Api;
using OrderPoint.Admin.Deliveries.Api.Requests;
using OrderPoint.Admin.Deliveries.Dialogs;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Deliveries.Services;

// Records a delivery the same way from the Deliveries page and from the reorder list, which fills it in
internal sealed class DeliveryDialogService(
    IDialogService dialogService,
    ApiService apiService,
    DeliveryApiClient deliveryApiClient)
{
    internal async Task<bool> RecordAsync(
        SupplierOptionDto? supplier = null,
        IReadOnlyList<DeliveryDraftLineDto>? lines = null)
    {
        var parameters = new DialogParameters<RecordDeliveryDialog>
        {
            { dialog => dialog.Supplier, supplier },
            { dialog => dialog.Lines, lines ?? [] }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true
        };

        IDialogReference dialogReference = await dialogService
            .ShowAsync<RecordDeliveryDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return false;
        }

        var request = (dialogResult.Data as CreateDeliveryRequest)!;

        decimal total = request.Lines.Sum(line => line.Packs * line.PackCost);

        return await apiService.ExecuteAsync(
            () => deliveryApiClient.CreateDeliveryAsync(request),
            $"Delivery recorded: {total.ToString("C", CultureInfo.CurrentCulture)}");
    }
}