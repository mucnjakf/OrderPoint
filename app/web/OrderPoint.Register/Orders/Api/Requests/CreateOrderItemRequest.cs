namespace OrderPoint.Register.Orders.Api.Requests;

internal sealed record CreateOrderItemRequest(Guid ItemId, int Quantity);