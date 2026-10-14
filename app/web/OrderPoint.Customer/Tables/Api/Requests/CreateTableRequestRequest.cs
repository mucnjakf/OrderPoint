using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Customer.Tables.Api.Requests;

internal sealed record CreateTableRequestRequest(TableRequestType Type);