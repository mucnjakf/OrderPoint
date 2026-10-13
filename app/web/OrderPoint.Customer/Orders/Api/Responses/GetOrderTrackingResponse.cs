using OrderPoint.Customer.Orders.Dtos;

namespace OrderPoint.Customer.Orders.Api.Responses;

internal sealed record GetOrderTrackingResponse(OrderTrackingDto Data);