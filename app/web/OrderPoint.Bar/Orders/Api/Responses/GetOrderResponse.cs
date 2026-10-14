using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Bar.Orders.Api.Responses;

internal sealed record GetOrderResponse(OrderDto Data);