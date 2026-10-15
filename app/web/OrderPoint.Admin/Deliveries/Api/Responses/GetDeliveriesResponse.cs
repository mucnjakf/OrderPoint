using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Deliveries.Api.Responses;

internal sealed record GetDeliveriesResponse(PaginationDto<DeliveryDto> Data);