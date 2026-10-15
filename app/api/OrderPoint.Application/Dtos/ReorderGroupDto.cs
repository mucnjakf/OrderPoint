namespace OrderPoint.Application.Dtos;

/// <summary>What to order from one supplier.</summary>
/// <param name="SupplierId">The supplier's id, or `null` for stock items without a supplier.</param>
/// <param name="SupplierName">The supplier's name, or `null`.</param>
/// <param name="Lines">The stock items to order, by group and name.</param>
/// <param name="EstimatedCost">The lines' estimated cost, in euros.</param>
public sealed record ReorderGroupDto(
    Guid? SupplierId,
    string? SupplierName,
    IReadOnlyList<ReorderLineDto> Lines,
    decimal EstimatedCost);