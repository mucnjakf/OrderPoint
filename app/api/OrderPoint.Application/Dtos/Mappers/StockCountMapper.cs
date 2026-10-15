using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class StockCountMapper
{
    private const int MoneyDecimals = 2;

    internal static StockCountDto ToStockCountDto(this StockCount stockCount)
    {
        bool isOpen = stockCount.Status == StockCountStatus.Open;

        return new StockCountDto(
            stockCount.Id,
            stockCount.Status,
            stockCount.Groups,
            stockCount.Note,
            stockCount.Lines.Count,
            stockCount.Lines.Count(line => line.CountedQuantity is not null),
            RoundMoney(stockCount.ValueChange),
            stockCount.Lines
                .OrderBy(line => line.StockItem.Group)
                .ThenBy(line => line.StockItem.Name)
                .Select(line => line.ToStockCountLineDto(isOpen))
                .ToList(),
            stockCount.CreatedAtUtc,
            stockCount.ClosedAtUtc);
    }

    // While the count is open, a line not counted yet shows what is on the shelf now
    private static StockCountLineDto ToStockCountLineDto(this StockCountLine line, bool isOpen)
    {
        bool showsCurrent = isOpen && line.CountedQuantity is null;

        return new StockCountLineDto(
            line.Id,
            line.StockItemId,
            line.StockItem.Name,
            line.StockItem.Group,
            line.StockItem.Unit,
            line.StockItem.PackName,
            line.StockItem.PackSize,
            showsCurrent ? line.StockItem.Quantity : line.ExpectedQuantity,
            line.CountedQuantity,
            showsCurrent ? line.StockItem.UnitCost : line.UnitCost,
            line.QuantityChange,
            line.ValueChange is { } valueChange ? RoundMoney(valueChange) : null);
    }

    private static decimal RoundMoney(decimal amount)
        => Math.Round(amount, MoneyDecimals, MidpointRounding.AwayFromZero);
}