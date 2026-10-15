using MudBlazor;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class StockGroupExtensions
{
    internal static string GetIcon(this StockGroup group)
    {
        return group switch
        {
            StockGroup.Spirits => Icons.Material.Filled.LocalBar,
            StockGroup.Beer => Icons.Material.Filled.SportsBar,
            StockGroup.Wine => Icons.Material.Filled.WineBar,
            StockGroup.Mixers => Icons.Material.Filled.LocalDrink,
            StockGroup.Syrups => Icons.Material.Filled.WaterDrop,
            StockGroup.Garnish => Icons.Material.Filled.Spa,
            _ => Icons.Material.Filled.Inventory2
        };
    }
}