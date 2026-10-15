using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Deliveries.Dtos;

public sealed record DeliveryLineDto(
    Guid StockItemId,
    string StockItemName,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal Packs,
    decimal PackCost,
    decimal Total);