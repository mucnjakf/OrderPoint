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
            StockMovementType.Sale => "Sale",
            StockMovementType.Delivery => "Delivery",
            StockMovementType.Count => "Count",
            _ => "Correction"
        };
    }

    internal static Color GetColor(this StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Opening => Color.Info,
            StockMovementType.Waste => Color.Error,
            StockMovementType.Sale => Color.Success,
            StockMovementType.Delivery => Color.Secondary,
            StockMovementType.Count => Color.Primary,
            _ => Color.Warning
        };
    }

    internal static string GetIcon(this StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Opening => Icons.Material.Filled.Inventory,
            StockMovementType.Waste => Icons.Material.Filled.DeleteSweep,
            StockMovementType.Sale => Icons.Material.Filled.PointOfSale,
            StockMovementType.Delivery => Icons.Material.Filled.LocalShipping,
            StockMovementType.Count => Icons.Material.Filled.FactCheck,
            _ => Icons.Material.Filled.Tune
        };
    }
}