using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Admin.Orders.Api.Responses;

internal sealed record GetOrderResponse(OrderDto Data);