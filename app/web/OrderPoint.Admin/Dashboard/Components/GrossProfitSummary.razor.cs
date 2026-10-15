using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

// Revenue, the cost of what was sold and the profit between them, with what was lost from the stock
public sealed partial class GrossProfitSummary
{
    private const decimal FullShare = 1;

    [Parameter]
    [EditorRequired]
    public GrossProfitDto GrossProfit { get; set; } = null!;

    private decimal Margin => GrossProfit.Revenue == 0 ? 0 : GrossProfit.GrossProfit / GrossProfit.Revenue;

    private decimal CostsKnownShare => GrossProfit.Revenue == 0
        ? FullShare
        : GrossProfit.CostedRevenue / GrossProfit.Revenue;

    // Items without a recipe count as free, so the margin is too high until every item has one
    private string CostsKnownText => CostsKnownShare >= FullShare
        ? "Costs known for all revenue"
        : $"Costs known for {CostsKnownShare.ToString("P0", CultureInfo.CurrentCulture)} of revenue";

    private string CostsKnownStyle => CostsKnownShare >= FullShare
        ? "color: var(--mud-palette-text-secondary)"
        : "color: var(--mud-palette-warning); font-weight: 600";

    private decimal WasteChange => -GrossProfit.WasteValue;

    private decimal? ChangePercent => GrossProfit.PreviousGrossProfit == 0
        ? null
        : (GrossProfit.GrossProfit - GrossProfit.PreviousGrossProfit) / Math.Abs(GrossProfit.PreviousGrossProfit) * 100;

    private Color ChangeColor => ChangePercent switch
    {
        > 0 => Color.Success,
        < 0 => Color.Error,
        _ => Color.Default
    };

    private string ChangeIcon => ChangePercent switch
    {
        > 0 => Icons.Material.Filled.TrendingUp,
        < 0 => Icons.Material.Filled.TrendingDown,
        _ => Icons.Material.Filled.TrendingFlat
    };

    private string ChangeText => ChangePercent is null ? "No data" : $"{Math.Abs(ChangePercent.Value):0}%";

    private string GetShareText(decimal amount)
    {
        decimal share = GrossProfit.Revenue == 0 ? 0 : amount / GrossProfit.Revenue;

        return share.ToString("P0", CultureInfo.CurrentCulture);
    }
}