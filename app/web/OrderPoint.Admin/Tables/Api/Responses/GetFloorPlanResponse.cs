using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Admin.Tables.Api.Responses;

internal sealed record GetFloorPlanResponse(IReadOnlyList<AreaDto> Data);