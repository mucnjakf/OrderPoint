using OrderPoint.Bartender.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Api.Responses;

internal sealed record GetTablesStatusResponse(IReadOnlyList<TableStatusDto> Data);