using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Dtos;

namespace OrderPoint.Admin.Stock.Components;

// The stock item at the top of the delete, waste and correction dialogs
public sealed partial class StockItemSummaryCard
{
    [Parameter]
    [EditorRequired]
    public StockItemDto StockItem { get; set; } = null!;

    private string QuantityText
    {
        get
        {
            string value = StockItem.Value.ToString("C", CultureInfo.CurrentCulture);

            return $"{StockItem.GetQuantityText(StockItem.Quantity)} · {value}";
        }
    }
}