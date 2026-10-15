using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Admin.Deliveries.Services;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Stock.Dialogs;

// What to order, by supplier; "Record delivery" opens the delivery form with the suggestions filled in
public sealed partial class ReorderDialog
{
    private const string SecondaryStyle = "color: var(--mud-palette-text-secondary)";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Inject]
    private DeliveryDialogService DeliveryDialogService { get; set; } = null!;

    [Parameter]
    public EventCallback OnStockChanged { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private IReadOnlyList<ReorderGroupDto> Groups { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetReorderListAsync();
    }

    private async Task GetReorderListAsync()
    {
        IReadOnlyList<ReorderGroupDto>? groups = await ApiService.ExecuteAsync(
            () => StockApiClient.GetReorderListAsync());

        Groups = groups ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task RecordDeliveryAsync(ReorderGroupDto group)
    {
        SupplierOptionDto? supplier = group.SupplierId is { } supplierId
            ? new SupplierOptionDto(supplierId, group.SupplierName!)
            : null;

        List<DeliveryDraftLineDto> lines = group.Lines
            .Select(line => new DeliveryDraftLineDto(
                new DeliveryStockItemDto(
                    line.StockItemId,
                    line.Name,
                    line.Group,
                    line.Unit,
                    line.PackName,
                    line.PackSize,
                    line.PackCost),
                line.SuggestedPacks))
            .ToList();

        if (!await DeliveryDialogService.RecordAsync(supplier, lines))
        {
            return;
        }

        await GetReorderListAsync();
        await OnStockChanged.InvokeAsync();
    }

    // "3 stock items · about €184.00"
    private static string GetGroupText(ReorderGroupDto group)
    {
        string cost = group.EstimatedCost.ToString("C", CultureInfo.CurrentCulture);

        return $"{group.Lines.Count.ToCountText("stock item", "stock items")} · about {cost}";
    }

    private static string? GetQuantityStyle(ReorderLineDto line)
    {
        return line.Level is StockLevel.Out ? "color: var(--mud-palette-error); font-weight: 600" : null;
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}