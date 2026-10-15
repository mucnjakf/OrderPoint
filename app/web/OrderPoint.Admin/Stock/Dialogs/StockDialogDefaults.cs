using System.Globalization;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Stock.Dialogs;

// Shared by the create and update dialogs
internal static class StockDialogDefaults
{
    internal static string GetUnitLabel(StockUnit unit)
    {
        return unit switch
        {
            StockUnit.Milliliter => "Milliliters (ml)",
            StockUnit.Gram => "Grams (g)",
            _ => "Pieces"
        };
    }

    internal static string GetPackName(StockUnit unit)
    {
        return unit switch
        {
            StockUnit.Milliliter => "bottle",
            StockUnit.Gram => "bag",
            _ => "piece"
        };
    }

    internal static decimal GetPackSize(StockUnit unit)
    {
        return unit switch
        {
            StockUnit.Milliliter => 700,
            StockUnit.Gram => 1000,
            _ => 1
        };
    }

    // "On the shelf: 2 bottles · 1.4 L, worth €28.00. Low at or below 1 bottle."
    internal static string GetPreviewText(StockItemDto stockItem)
    {
        string value = Math.Max(stockItem.Value, 0).ToString("C", CultureInfo.CurrentCulture);
        string text = $"On the shelf: {stockItem.GetQuantityText(stockItem.Quantity)}, worth {value}.";

        return stockItem.ParLevel > 0
            ? $"{text} Low at or below {stockItem.GetPacksText(stockItem.ParLevel)}."
            : text;
    }
}