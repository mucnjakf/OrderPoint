using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Stock.Dtos;

public sealed record ReorderLineDto(
    Guid StockItemId,
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal Quantity,
    decimal ParLevel,
    StockLevel Level,
    decimal SuggestedPacks,
    decimal EstimatedCost);