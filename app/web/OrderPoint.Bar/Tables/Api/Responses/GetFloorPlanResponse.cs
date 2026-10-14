using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bar.Tables.Api.Responses;

internal sealed record GetFloorPlanResponse(IReadOnlyList<AreaDto> Data);