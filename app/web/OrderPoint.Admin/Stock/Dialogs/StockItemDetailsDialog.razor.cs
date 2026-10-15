using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Services;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Stock.Dialogs;

// The stock item with its history; waste and corrections recorded here show at once
public sealed partial class StockItemDetailsDialog
{
    private const int PageSize = 10;

    private const string FactsStyle = "display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px;";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Inject]
    private StockDialogService StockDialogService { get; set; } = null!;

    [Parameter]
    public StockItemDto StockItem { get; set; } = null!;

    [Parameter]
    public EventCallback OnStockChanged { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // The parameter is reset whenever the dialog provider re-renders, so reloads go here
    private StockItemDto CurrentStockItem { get; set; } = null!;

    private PaginationDto<StockMovementDto>? Pagination { get; set; }

    private IReadOnlyList<StockMovementDto> Movements { get; set; } = [];

    private bool IsLoadingMovements { get; set; } = true;

    // "Spirits · in milliliters (ml)"
    private string SubtitleText =>
        $"{CurrentStockItem.Group} · in {StockDialogDefaults.GetUnitLabel(CurrentStockItem.Unit).ToLowerInvariant()}";

    private IReadOnlyList<(string Icon, string Label, string Value)> Facts =>
    [
        (Icons.Material.Filled.Inventory, "On the shelf", CurrentStockItem.GetQuantityText(CurrentStockItem.Quantity)),
        (Icons.Material.Filled.Euro, "Value", CurrentStockItem.Value.ToString("C", CultureInfo.CurrentCulture)),
        (Icons.Material.Filled.Flag, "Par level", CurrentStockItem.ParLevel > 0
            ? CurrentStockItem.GetPacksText(CurrentStockItem.ParLevel)
            : "-"),
        (Icons.Material.Filled.LocalShipping, "Pack", CurrentStockItem.GetPackText())
    ];

    protected override async Task OnInitializedAsync()
    {
        CurrentStockItem = StockItem;

        await GetMovementsAsync(pageNumber: 1);
    }

    private async Task GetStockItemAsync()
    {
        StockItemDto? stockItem = await ApiService.ExecuteAsync(
            () => StockApiClient.GetStockItemAsync(CurrentStockItem.Id));

        if (stockItem is null)
        {
            return;
        }

        CurrentStockItem = stockItem;

        StateHasChanged();
    }

    private async Task GetMovementsAsync(int pageNumber)
    {
        IsLoadingMovements = true;

        Pagination = await ApiService.ExecuteAsync(
            () => StockApiClient.GetStockMovementsAsync(CurrentStockItem.Id, pageNumber, PageSize));

        Movements = Pagination?.Items ?? [];

        IsLoadingMovements = false;

        StateHasChanged();
    }

    private async Task RecordWasteAsync()
    {
        bool isSuccess = await StockDialogService.RecordWasteAsync(CurrentStockItem);

        await OnChangedAsync(isSuccess);
    }

    private async Task CorrectAsync()
    {
        bool isSuccess = await StockDialogService.CorrectAsync(CurrentStockItem);

        await OnChangedAsync(isSuccess);
    }

    private async Task OnChangedAsync(bool isSuccess)
    {
        if (!isSuccess)
        {
            return;
        }

        await Task.WhenAll(GetStockItemAsync(), GetMovementsAsync(pageNumber: 1));
        await OnStockChanged.InvokeAsync();
    }

    // "+2 bottles · 1.4 L", "−50 ml"
    private string GetChangeText(StockMovementDto movement)
    {
        string text = CurrentStockItem.GetQuantityText(movement.QuantityChange);

        return movement.QuantityChange > 0 ? $"+{text}" : text;
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}