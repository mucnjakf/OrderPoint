using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Admin.Payments.Api.Responses;

internal sealed record GetPaymentsResponse(PaginationDto<OrderDto> Data);