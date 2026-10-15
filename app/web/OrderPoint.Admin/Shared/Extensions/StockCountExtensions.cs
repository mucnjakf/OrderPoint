using System.Globalization;
using MudBlazor;
using OrderPoint.Admin.StockCounts.Dtos;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class StockCountExtensions
{
    // "Spirits, Beer", or "All groups"
    internal static string GetGroupsText(this StockCountDto stockCount, int allGroupsCount)
    {
        return stockCount.Groups.Count == allGroupsCount ? "All groups" : string.Join(", ", stockCount.Groups);
    }

    // "3.4 bottles · 2.38 L"; a pack of one base unit needs only the packs
    internal static string GetQuantityText(this StockCountLineDto line, decimal quantity)
    {
        string packsText = (quantity / line.PackSize).FormatPacks(line.PackName);

        return line.PackSize == 1 ? packsText : $"{packsText} · {line.Unit.FormatAmount(quantity)}";
    }

    // "−80 ml", "+2 pieces", or "No difference"
    internal static string GetQuantityChangeText(this StockCountLineDto line)
    {
        if (line.QuantityChange is not { } change)
        {
            return "-";
        }

        if (change == 0)
        {
            return "No difference";
        }

        string sign = change > 0 ? "+" : "−";

        return $"{sign}{line.Unit.FormatAmount(Math.Abs(change))}";
    }

    // "−€1.60", "+€4.20"
    internal static string ToSignedMoneyText(this decimal amount)
    {
        string money = Math.Abs(amount).ToString("C", CultureInfo.CurrentCulture);

        return amount switch
        {
            > 0 => $"+{money}",
            < 0 => $"−{money}",
            _ => money
        };
    }

    // A loss in red, a gain in green
    internal static string GetValueChangeStyle(this decimal amount)
    {
        return amount switch
        {
            < 0 => "color: var(--mud-palette-error); font-weight: 600",
            > 0 => "color: var(--mud-palette-success); font-weight: 600",
            _ => "color: var(--mud-palette-text-secondary)"
        };
    }

    internal static Color GetValueChangeColor(this decimal amount)
    {
        return amount switch
        {
            < 0 => Color.Error,
            > 0 => Color.Success,
            _ => Color.Default
        };
    }
}