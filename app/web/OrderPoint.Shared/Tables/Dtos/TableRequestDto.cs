using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Shared.Tables.Dtos;

public sealed record TableRequestDto(
    Guid Id,
    Guid TableId,
    string TableCode,
    TableRequestType Type,
    TableRequestStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DoneAtUtc);