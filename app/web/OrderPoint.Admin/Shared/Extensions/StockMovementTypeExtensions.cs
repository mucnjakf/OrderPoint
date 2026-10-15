using MudBlazor;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class StockMovementTypeExtensions
{
    internal static string GetLabel(this StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Opening => "Opening stock",
            StockMovementType.Waste => "Waste",
            _ => "Correction"
        };
    }

    internal static Color GetColor(this StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Opening => Color.Info,
            StockMovementType.Waste => Color.Error,
            _ => Color.Warning
        };
    }

    internal static string GetIcon(this StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Opening => Icons.Material.Filled.Inventory,
            StockMovementType.Waste => Icons.Material.Filled.DeleteSweep,
            _ => Icons.Material.Filled.Tune
        };
    }
}