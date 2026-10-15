using MudBlazor;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class StockLevelExtensions
{
    internal static string GetLabel(this StockLevel level)
    {
        return level switch
        {
            StockLevel.Out => "Out",
            StockLevel.Low => "Low",
            _ => "In stock"
        };
    }

    internal static Color GetColor(this StockLevel level)
    {
        return level switch
        {
            StockLevel.Out => Color.Error,
            StockLevel.Low => Color.Warning,
            _ => Color.Success
        };
    }
}