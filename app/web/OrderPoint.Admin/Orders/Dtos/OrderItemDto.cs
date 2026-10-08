namespace OrderPoint.Admin.Orders.Dtos;

public sealed record OrderItemDto(
    Guid Id,
    Guid ItemId,
    string Name,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice);