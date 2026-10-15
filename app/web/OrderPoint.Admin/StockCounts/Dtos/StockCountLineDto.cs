using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.StockCounts.Dtos;

public sealed record StockCountLineDto(
    Guid Id,
    Guid StockItemId,
    string StockItemName,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal? ExpectedQuantity,
    decimal? CountedQuantity,
    decimal? UnitCost,
    decimal? QuantityChange,
    decimal? ValueChange);