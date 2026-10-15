namespace OrderPoint.Admin.Stock.Dtos;

public sealed record ReorderGroupDto(
    Guid? SupplierId,
    string? SupplierName,
    IReadOnlyList<ReorderLineDto> Lines,
    decimal EstimatedCost);