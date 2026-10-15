using System.Globalization;
using OrderPoint.Admin.Stock.Dtos;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class StockItemDtoExtensions
{
    // "3.4 kegs · 102 L"; a pack of one base unit needs only the packs
    internal static string GetQuantityText(this StockItemDto stockItem, decimal quantity)
    {
        string packsText = stockItem.GetPacksText(quantity);

        return stockItem.PackSize == 1 ? packsText : $"{packsText} · {stockItem.Unit.FormatAmount(quantity)}";
    }

    // "3.4 kegs", "1 bottle"
    internal static string GetPacksText(this StockItemDto stockItem, decimal quantity)
    {
        decimal packs = quantity / stockItem.PackSize;

        return $"{packs.ToString("0.#", CultureInfo.CurrentCulture)} {GetPackName(stockItem.PackName, packs)}";
    }

    // "bottle · 700 ml · €14.00"
    internal static string GetPackText(this StockItemDto stockItem)
    {
        string cost = stockItem.PackCost.ToString("C", CultureInfo.CurrentCulture);

        return $"{stockItem.PackName} · {stockItem.Unit.FormatAmount(stockItem.PackSize)} · {cost}";
    }

    // A simple English plural is enough for packs: bottles, kegs, boxes
    private static string GetPackName(string packName, decimal packs)
    {
        if (Math.Abs(packs) == 1)
        {
            return packName;
        }

        bool needsEs = packName.EndsWith('s') || packName.EndsWith('x') || packName.EndsWith("ch")
                       || packName.EndsWith("sh");

        return needsEs ? $"{packName}es" : $"{packName}s";
    }
}