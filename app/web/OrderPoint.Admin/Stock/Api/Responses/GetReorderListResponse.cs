using OrderPoint.Admin.Stock.Dtos;

namespace OrderPoint.Admin.Stock.Api.Responses;

internal sealed record GetReorderListResponse(IReadOnlyList<ReorderGroupDto> Data);