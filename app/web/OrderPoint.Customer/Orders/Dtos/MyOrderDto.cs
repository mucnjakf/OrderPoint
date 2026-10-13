namespace OrderPoint.Customer.Orders.Dtos;

// What the browser keeps about an order placed on this phone
public sealed record MyOrderDto(Guid Id, DateTimeOffset PlacedAtUtc);