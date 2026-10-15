using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.Stock.Dtos;

public sealed record StockMovementDto(
    Guid Id,
    StockMovementType Type,
    decimal QuantityChange,
    decimal QuantityAfter,
    decimal ValueChange,
    string? Note,
    DateTimeOffset CreatedAtUtc);