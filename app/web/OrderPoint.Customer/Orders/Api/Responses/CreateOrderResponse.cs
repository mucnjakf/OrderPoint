using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Customer.Orders.Api.Responses;

internal sealed record CreateOrderResponse(OrderDto Data);