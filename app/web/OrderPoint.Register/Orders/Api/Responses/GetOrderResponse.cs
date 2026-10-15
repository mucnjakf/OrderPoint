using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Register.Orders.Api.Responses;

internal sealed record GetOrderResponse(OrderDto Data);