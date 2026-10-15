using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Suppliers.Api;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Stock.Dialogs;

public sealed partial class CreateStockItemDialog
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private SupplierApiClient SupplierApiClient { get; set; } = null!;
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CreateStockItemRequest Request { get; set; } = new()
    {
        Group = StockGroup.Spirits,
        Unit = StockUnit.Milliliter,
        PackName = StockDialogDefaults.GetPackName(StockUnit.Milliliter),
        PackSize = StockDialogDefaults.GetPackSize(StockUnit.Milliliter),
        Status = StockItemStatus.Active
    };

    // What the numbers mean, as the stock sheet will show them
    private string? PreviewText
    {
        get
        {
            if (Request.PackSize <= 0 || string.IsNullOrWhiteSpace(Request.PackName))
            {
                return null;
            }

            var preview = new StockItemDto(
                Guid.Empty,
                Request.Name,
                Request.Group,
                Request.Unit,
                Request.PackName,
                Request.PackSize,
                Request.PackCost,
                Request.ParLevel,
                Request.OpeningQuantity,
                Request.OpeningQuantityInPacks * Request.PackCost,
                StockLevel.Ok,
                Request.Status,
                Request.SupplierId,
                Request.Supplier?.Name,
                DateTimeOffset.UtcNow,
                UpdatedAtUtc: null);

            return StockDialogDefaults.GetPreviewText(preview);
        }
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

    private static string GetUnitLabel(StockUnit unit)
    {
        return StockDialogDefaults.GetUnitLabel(unit);
    }

    // A unit comes with its usual pack: a 700 ml bottle, a 1 kg bag, a single piece
    private void OnUnitChanged()
    {
        Request.PackName = StockDialogDefaults.GetPackName(Request.Unit);
        Request.PackSize = StockDialogDefaults.GetPackSize(Request.Unit);
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