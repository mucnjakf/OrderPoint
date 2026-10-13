using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Bartender.Orders.Api.Responses;

internal sealed record GetOrderResponse(OrderDto Data);