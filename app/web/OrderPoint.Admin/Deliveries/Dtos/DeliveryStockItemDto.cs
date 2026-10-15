using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Deliveries.Dtos;

public sealed record DeliveryStockItemDto(
    Guid Id,
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost);