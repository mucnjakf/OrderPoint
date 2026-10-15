namespace OrderPoint.Bartender.Orders.Api.Requests;

// Without a table code the order is at the counter
internal sealed record CreateBartenderOrderRequest(
    string? TableCode,
    string? CustomerName,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items);