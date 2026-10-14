using OrderPoint.Customer.Tables.Dtos;

namespace OrderPoint.Customer.Tables.Api.Responses;

internal sealed record GetTableByCodeResponse(GuestTableDto Data);