using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Deliveries.Api.Requests;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Stock.Sorting;
using OrderPoint.Admin.Suppliers.Api;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Deliveries.Dialogs;

// What arrived, line by line; returns the request, the caller saves it
public sealed partial class RecordDeliveryDialog
{
    // Like the API
    private const int MaxLines = 50;

    private const int SearchResultsCount = 10;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private SupplierApiClient SupplierApiClient { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    // From the reorder list: the supplier and the suggested lines
    [Parameter]
    public SupplierOptionDto? Supplier { get; set; }

    [Parameter]
    public IReadOnlyList<DeliveryDraftLineDto> Lines { get; set; } = [];

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private SupplierOptionDto? SelectedSupplier { get; set; }

    private DateTime? DeliveredDate { get; set; }

    private string? Reference { get; set; }

    private string? Note { get; set; }

    private List<DeliveryRowState> Rows { get; set; } = [];

    // In the viewer's time zone
    private DateTime Today => DateTimeOffset.UtcNow.ToTimeZone(TimeZoneService.TimeZone).Date;

    private decimal Total => Rows.Sum(row => row.Packs * row.PackCost);

    // A supplier, a day, and every line picked with its packs, each stock item once
    private bool CanSave => SelectedSupplier is not null
                            && DeliveredDate is not null
                            && Rows.All(row => row.StockItem is not null && row.Packs > 0 && row.PackCost >= 0)
                            && !Rows.Any(IsDuplicate);

    protected override void OnInitialized()
    {
        SelectedSupplier = Supplier;
        DeliveredDate = Today;

        Rows = Lines
            .Select(line => new DeliveryRowState
            {
                StockItem = line.StockItem,
                Packs = line.Packs,
                PackCost = line.StockItem.PackCost
            })
            .ToList();

        if (Rows.Count == 0)
        {
            AddRow();
        }
    }

    private void AddRow()
    {
        Rows.Add(new DeliveryRowState());
    }

    // The price starts at what a pack costs now
    private void OnStockItemChanged(DeliveryRowState row)
    {
        row.PackCost = row.StockItem?.PackCost ?? 0;
    }

    private bool IsDuplicate(DeliveryRowState row)
    {
        return row.StockItem is not null
               && Rows.Any(other => other != row && other.StockItem?.Id == row.StockItem.Id);
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

    private async Task<IEnumerable<DeliveryStockItemDto>> SearchStockItemsAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        PaginationDto<StockItemDto>? stockItems = await ApiService.ExecuteAsync(
            () => StockApiClient.GetStockItemsAsync(
                pageNumber: 1,
                SearchResultsCount,
                nameof(StockItemSortBy.NameAsc),
                value,
                status: StockItemStatus.Active,
                cancellationToken: cancellationToken),
            cancellationToken);

        return (stockItems?.Items ?? []).Select(stockItem => new DeliveryStockItemDto(
            stockItem.Id,
            stockItem.Name,
            stockItem.Group,
            stockItem.Unit,
            stockItem.PackName,
            stockItem.PackSize,
            stockItem.PackCost));
    }

    private void Save()
    {
        var request = new CreateDeliveryRequest(
            SelectedSupplier!.Id,
            DateOnly.FromDateTime(DeliveredDate!.Value),
            string.IsNullOrWhiteSpace(Reference) ? null : Reference,
            string.IsNullOrWhiteSpace(Note) ? null : Note,
            Rows.Select(row => new CreateDeliveryLineRequest(row.StockItem!.Id, row.Packs, row.PackCost)).ToList());

        MudDialogInstance.Close(DialogResult.Ok(request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }

    private sealed class DeliveryRowState
    {
        public DeliveryStockItemDto? StockItem { get; set; }

        public decimal Packs { get; set; }

        public decimal PackCost { get; set; }
    }
}