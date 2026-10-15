using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Deliveries.Api;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Admin.Deliveries.Sorting;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Sorting;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Suppliers.Dialogs;

// The supplier's contact details, what is bought from them and what they recently delivered
public sealed partial class SupplierDetailsDialog
{
    private const string SecondaryStyle = "color: var(--mud-palette-text-secondary)";

    // A supplier delivers a few dozen stock items at most
    private const int StockItemsCount = 100;

    private const int RecentDeliveriesCount = 5;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Inject]
    private DeliveryApiClient DeliveryApiClient { get; set; } = null!;

    [Parameter]
    public SupplierDto Supplier { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private IReadOnlyList<StockItemDto> StockItems { get; set; } = [];

    private IReadOnlyList<DeliveryDto> Deliveries { get; set; } = [];

    private bool IsLoadingStockItems { get; set; } = true;

    private bool IsLoadingDeliveries { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(GetStockItemsAsync(), GetDeliveriesAsync());
    }

    private async Task GetStockItemsAsync()
    {
        PaginationDto<StockItemDto>? stockItems = await ApiService.ExecuteAsync(() => StockApiClient.GetStockItemsAsync(
            pageNumber: 1,
            StockItemsCount,
            nameof(StockItemSortBy.NameAsc),
            supplierId: Supplier.Id));

        StockItems = stockItems?.Items ?? [];

        IsLoadingStockItems = false;

        StateHasChanged();
    }

    private async Task GetDeliveriesAsync()
    {
        PaginationDto<DeliveryDto>? deliveries = await ApiService.ExecuteAsync(
            () => DeliveryApiClient.GetDeliveriesAsync(
                pageNumber: 1,
                RecentDeliveriesCount,
                nameof(DeliverySortBy.DeliveredOnDesc),
                supplierId: Supplier.Id));

        Deliveries = deliveries?.Items ?? [];

        IsLoadingDeliveries = false;

        StateHasChanged();
    }

    // "INV-2231 · 5 lines"
    private static string GetDeliveryText(DeliveryDto delivery)
    {
        return $"{delivery.Reference ?? "-"} · {delivery.Lines.Count.ToCountText("line", "lines")}";
    }

    private static string GetDateText(DateOnly date)
    {
        return date.ToString(CultureInfo.CurrentCulture.GetShortMonthDayPattern(), CultureInfo.CurrentCulture);
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}