using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class SupplierMapper
{
    internal static SupplierDto ToSupplierDto(
        this Supplier supplier,
        int stockItemsCount,
        int reorderCount,
        DateOnly? lastDeliveredOn) => new(
        supplier.Id,
        supplier.Name,
        supplier.ContactName,
        supplier.Phone,
        supplier.Email,
        supplier.Note,
        supplier.Status,
        stockItemsCount,
        reorderCount,
        lastDeliveredOn,
        supplier.CreatedAtUtc,
        supplier.UpdatedAtUtc);
}