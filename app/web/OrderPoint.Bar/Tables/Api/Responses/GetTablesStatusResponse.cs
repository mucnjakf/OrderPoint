using OrderPoint.Bar.Tables.Dtos;

namespace OrderPoint.Bar.Tables.Api.Responses;

internal sealed record GetTablesStatusResponse(IReadOnlyList<TableStatusDto> Data);