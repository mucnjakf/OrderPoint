using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Stock.Components;

// One stock item on the sheet: name, level against par, quantity in packs and base units, par, value, actions
public sealed partial class StockItemRow
{
    [Parameter]
    [EditorRequired]
    public StockItemDto StockItem { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<StockItemDto> OnWasteClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<StockItemDto> OnCorrectClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<StockItemDto> OnEditClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<StockItemDto> OnDeleteClick { get; set; }

    private string QuantityStyle => StockItem.Level is StockLevel.Ok
        ? string.Empty
        : $"color: var(--mud-palette-{(StockItem.Level is StockLevel.Out ? "error" : "warning")}); font-weight: 600";
}