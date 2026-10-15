using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Api.Responses;

internal sealed record GetFloorPlanResponse(IReadOnlyList<AreaDto> Data);