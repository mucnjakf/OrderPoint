using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class DeliveryMapper
{
    private const int MoneyDecimals = 2;

    // Enough for twice the par level; without a par level one pack
    private const decimal TargetParLevels = 2;

    internal static DeliveryDto ToDeliveryDto(this Delivery delivery) => new(
        delivery.Id,
        new DeliverySupplierDto(delivery.Supplier.Id, delivery.Supplier.Name),
        delivery.DeliveredOn,
        delivery.Reference,
        delivery.Note,
        delivery.Lines
            .OrderBy(line => line.StockItem.Name)
            .Select(line => line.ToDeliveryLineDto())
            .ToList(),
        RoundMoney(delivery.Total),
        delivery.CreatedAtUtc);

    internal static ReorderLineDto ToReorderLineDto(this StockItem stockItem)
    {
        decimal needed = TargetParLevels * stockItem.ParLevel - Math.Max(stockItem.Quantity, 0);
        decimal suggestedPacks = Math.Max(Math.Ceiling(needed / stockItem.PackSize), 1);

        return new ReorderLineDto(
            stockItem.Id,
            stockItem.Name,
            stockItem.Group,
            stockItem.Unit,
            stockItem.PackName,
            stockItem.PackSize,
            stockItem.PackCost,
            stockItem.Quantity,
            stockItem.ParLevel,
            stockItem.Level,
            suggestedPacks,
            RoundMoney(suggestedPacks * stockItem.PackCost));
    }

    private static DeliveryLineDto ToDeliveryLineDto(this DeliveryLine line) => new(
        line.StockItemId,
        line.StockItem.Name,
        line.StockItem.Group,
        line.StockItem.Unit,
        line.StockItem.PackName,
        line.StockItem.PackSize,
        line.Packs,
        line.PackCost,
        RoundMoney(line.Total));

    private static decimal RoundMoney(decimal amount)
        => Math.Round(amount, MoneyDecimals, MidpointRounding.AwayFromZero);
}