using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class StockMapper
{
    private const int MoneyDecimals = 2;

    internal static StockItemDto ToStockItemDto(this StockItem stockItem) => new(
        stockItem.Id,
        stockItem.Name,
        stockItem.Group,
        stockItem.Unit,
        stockItem.PackName,
        stockItem.PackSize,
        stockItem.PackCost,
        stockItem.ParLevel,
        stockItem.Quantity,
        Math.Round(stockItem.Value, MoneyDecimals, MidpointRounding.AwayFromZero),
        stockItem.Level,
        stockItem.Status,
        stockItem.SupplierId,
        stockItem.Supplier?.Name,
        stockItem.CreatedAtUtc,
        stockItem.UpdatedAtUtc);

    internal static StockMovementDto ToStockMovementDto(this StockMovement stockMovement) => new(
        stockMovement.Id,
        stockMovement.Type,
        stockMovement.QuantityChange,
        stockMovement.QuantityAfter,
        Math.Round(stockMovement.QuantityChange * stockMovement.UnitCost, MoneyDecimals, MidpointRounding.AwayFromZero),
        stockMovement.Note,
        stockMovement.CreatedAtUtc);
}