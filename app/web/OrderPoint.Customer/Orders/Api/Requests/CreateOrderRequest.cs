using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Customer.Orders.Api.Requests;

internal sealed record CreateOrderRequest(
    string TableCode,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items,
    PaymentMethod PaymentMethod,
    CreateOrderCardRequest? Card);