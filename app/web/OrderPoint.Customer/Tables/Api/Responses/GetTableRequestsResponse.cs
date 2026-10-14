using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Customer.Tables.Api.Responses;

internal sealed record GetTableRequestsResponse(IReadOnlyList<TableRequestDto> Data);