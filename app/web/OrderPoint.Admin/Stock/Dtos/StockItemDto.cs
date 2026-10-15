using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Stock.Dtos;

public sealed record StockItemDto(
    Guid Id,
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal ParLevel,
    decimal Quantity,
    decimal Value,
    StockLevel Level,
    StockItemStatus Status,
    Guid? SupplierId,
    string? SupplierName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);