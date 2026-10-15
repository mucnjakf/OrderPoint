using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Dashboard.Components;

// What is out or running low, out first
public sealed partial class LowStockList
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<StockItemDto> StockItems { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public int OutCount { get; set; }

    [Parameter]
    [EditorRequired]
    public int LowCount { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<StockItemDto> OnStockItemClick { get; set; }

    private Guid? HoveredStockItemId { get; set; }

    private string? GetRowStyle(StockItemDto stockItem)
    {
        return stockItem.Id == HoveredStockItemId ? "background-color: var(--mud-palette-table-hover);" : null;
    }

    private static string GetQuantityStyle(StockItemDto stockItem)
    {
        string color = stockItem.Level is StockLevel.Out ? "error" : "warning";

        return $"color: var(--mud-palette-{color}); font-weight: 600; white-space: nowrap";
    }

    private async Task OnStockItemClickAsync(StockItemDto stockItem)
    {
        await OnStockItemClick.InvokeAsync(stockItem);
    }
}