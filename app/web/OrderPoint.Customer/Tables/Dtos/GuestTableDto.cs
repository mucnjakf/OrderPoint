using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Customer.Tables.Dtos;

public sealed record GuestTableDto(string Code, string AreaName, TableStatus Status);