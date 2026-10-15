using System.Globalization;
using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class StockUnitExtensions
{
    private const decimal LargeUnitFactor = 1000;

    internal static string GetSymbol(this StockUnit unit)
    {
        return unit switch
        {
            StockUnit.Milliliter => "ml",
            StockUnit.Gram => "g",
            _ => "pcs"
        };
    }

    // 420 ml, 1.5 L, 800 g, 2 kg, 38 pieces
    internal static string FormatAmount(this StockUnit unit, decimal amount)
    {
        bool isLarge = Math.Abs(amount) >= LargeUnitFactor;

        return unit switch
        {
            StockUnit.Milliliter when isLarge => $"{FormatNumber(amount / LargeUnitFactor, "0.##")} L",
            StockUnit.Milliliter => $"{FormatNumber(amount, "0.#")} ml",
            StockUnit.Gram when isLarge => $"{FormatNumber(amount / LargeUnitFactor, "0.##")} kg",
            StockUnit.Gram => $"{FormatNumber(amount, "0.#")} g",
            _ => $"{FormatNumber(amount, "0.#")} {(Math.Abs(amount) == 1 ? "piece" : "pieces")}"
        };
    }

    private static string FormatNumber(decimal number, string format)
    {
        return number.ToString(format, CultureInfo.CurrentCulture);
    }
}