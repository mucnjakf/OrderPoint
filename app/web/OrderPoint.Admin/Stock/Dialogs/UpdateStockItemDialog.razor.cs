using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Suppliers.Api;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Stock.Dialogs;

// The unit and the quantity stay: the quantity only changes through waste and corrections
public sealed partial class UpdateStockItemDialog
{
    private const int PacksDecimals = 3;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private SupplierApiClient SupplierApiClient { get; set; } = null!;

    [Parameter]
    public StockItemDto StockItem { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateStockItemRequest Request { get; set; } = null!;

    private string? PreviewText => Request.PackSize <= 0 || string.IsNullOrWhiteSpace(Request.PackName)
        ? null
        : StockDialogDefaults.GetPreviewText(StockItem with
        {
            PackName = Request.PackName,
            PackSize = Request.PackSize,
            PackCost = Request.PackCost,
            ParLevel = Request.ParLevel,
            Value = StockItem.Quantity / Request.PackSize * Request.PackCost
        });

    protected override void OnInitialized()
    {
        Request = new UpdateStockItemRequest
        {
            Name = StockItem.Name,
            Group = StockItem.Group,
            PackName = StockItem.PackName,
            PackSize = StockItem.PackSize,
            PackCost = StockItem.PackCost,
            ParLevelInPacks = Math.Round(StockItem.ParLevel / StockItem.PackSize, PacksDecimals),
            Status = StockItem.Status,
            Supplier = StockItem.SupplierId is { } supplierId
                ? new SupplierOptionDto(supplierId, StockItem.SupplierName!)
                : null
        };
    }

    private async Task<IEnumerable<SupplierOptionDto>> SearchSuppliersAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SupplierOptionDto>? suppliers = await ApiService.ExecuteAsync(
            () => SupplierApiClient.SearchSuppliersAsync(value, cancellationToken),
            cancellationToken);

        return suppliers ?? [];
    }

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}