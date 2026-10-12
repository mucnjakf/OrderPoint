using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Orders.Api.Responses;

internal sealed record GetOrdersResponse(PaginationDto<OrderDto> Data);