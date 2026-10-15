namespace OrderPoint.Admin.Deliveries.Dtos;

public sealed record DeliveryDto(
    Guid Id,
    DeliverySupplierDto Supplier,
    DateOnly DeliveredOn,
    string? Reference,
    string? Note,
    IReadOnlyList<DeliveryLineDto> Lines,
    decimal Total,
    DateTimeOffset CreatedAtUtc);