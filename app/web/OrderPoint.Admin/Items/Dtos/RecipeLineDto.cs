using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Items.Dtos;

public sealed record RecipeLineDto(
    Guid StockItemId,
    string StockItemName,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal Quantity,
    decimal Cost,
    bool HasServing);