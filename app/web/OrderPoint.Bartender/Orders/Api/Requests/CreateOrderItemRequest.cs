namespace OrderPoint.Bartender.Orders.Api.Requests;

internal sealed record CreateOrderItemRequest(Guid ItemId, int Quantity);