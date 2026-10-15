using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Register.Today.Api.Responses;

internal sealed record GetPaymentsResponse(PaginationDto<OrderDto> Data);