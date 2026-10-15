using OrderPoint.Admin.Suppliers.Enumerations;

namespace OrderPoint.Admin.Suppliers.Dtos;

public sealed record SupplierDto(
    Guid Id,
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Note,
    SupplierStatus Status,
    int StockItemsCount,
    int ReorderCount,
    DateOnly? LastDeliveredOn,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);