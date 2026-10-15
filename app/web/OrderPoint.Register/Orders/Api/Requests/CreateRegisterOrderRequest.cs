namespace OrderPoint.Register.Orders.Api.Requests;

// A sale at the counter, paid in the same step
internal sealed record CreateRegisterOrderRequest(
    string? CustomerName,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items,
    PayOrderRequest Payment);