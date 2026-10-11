using OrderPoint.Admin.Items.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class PriceAdjustmentModeExtensions
{
    private const decimal PercentBase = 100m;

    private const int PriceDecimals = 2;

    internal static string GetLabel(this PriceAdjustmentMode mode)
    {
        return mode switch
        {
            PriceAdjustmentMode.Set => "Set price",
            PriceAdjustmentMode.ChangeByPercent => "Change by %",
            PriceAdjustmentMode.ChangeByAmount => "Change by €",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    internal static decimal Apply(this PriceAdjustmentMode mode, decimal price, decimal value)
    {
        decimal newPrice = mode switch
        {
            PriceAdjustmentMode.Set => value,
            PriceAdjustmentMode.ChangeByPercent => price * (1 + value / PercentBase),
            PriceAdjustmentMode.ChangeByAmount => price + value,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        return Math.Round(newPrice, PriceDecimals, MidpointRounding.AwayFromZero);
    }
}