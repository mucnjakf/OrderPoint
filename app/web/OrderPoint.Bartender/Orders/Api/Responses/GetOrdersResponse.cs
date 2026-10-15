using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Bartender.Orders.Api.Responses;

internal sealed record GetOrdersResponse(PaginationDto<OrderDto> Data);