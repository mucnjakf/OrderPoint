using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Stock.Dtos;

namespace OrderPoint.Admin.Stock.Components;

// Full at twice the par level, with the par level marked in the middle, so low reads as "below the line"
public sealed partial class StockLevelBar
{
    private const string ParMarkerStyle =
        "position: absolute; left: 50%; top: -3px; bottom: -3px; width: 2px; " +
        "background-color: var(--mud-palette-text-secondary);";

    private const double FullBarParLevels = 2;

    private const double FullPercent = 100;

    [Parameter]
    [EditorRequired]
    public StockItemDto StockItem { get; set; } = null!;

    private double Percent
    {
        get
        {
            if (StockItem.Quantity <= 0)
            {
                return 0;
            }

            if (StockItem.ParLevel <= 0)
            {
                return FullPercent;
            }

            double percent = (double)(StockItem.Quantity / StockItem.ParLevel) / FullBarParLevels * FullPercent;

            return Math.Min(percent, FullPercent);
        }
    }
}